using System.Diagnostics;
using Neon.Common;

namespace Neon.Core.Engine;

// This class defines a background job-processing worker that runs
// continuously and pulls jobs from storage, executes them, and updates their status..

public class JobWorker : BackgroundService
{

    private readonly IJobManager _jobManager;
    private readonly IJobExecutor _jobExecutor;
    private readonly ILogger<JobWorker> _logger;
    // A unique identifier for worker instance (machine name + a Guid snippet)
    private readonly string _workerId;
    // Limits how many jobs can run parallel (default to 5)
    private readonly int _concurrency;
    // Jobs are leased for 5 minutes so multuple workers don't process the same job simultaneously
    private readonly TimeSpan _leaseDuration = TimeSpan.FromMinutes(5);

    // controls concurrency - only concurrency jobs can be processed at once.
    private readonly SemaphoreSlim _semaphore;

    public JobWorker(

        IJobManager jobManager,
        IJobExecutor jobExecutor,
        ILogger<JobWorker> logger,
        int concurrency = 5
    )
    {
        _jobManager = jobManager;
        _jobExecutor = jobExecutor;
        _logger = logger;
        _workerId = Environment.MachineName + "-" + Guid.NewGuid().ToString("N")[..8];
        _concurrency = concurrency;
        _semaphore = new SemaphoreSlim(concurrency, concurrency);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {


        _logger.LogInformation("Job worker {workerId} started with concurrency {concurrency} ", _workerId, _concurrency);

        var tasks = new List<Task>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {

                // waits for slot using _semaphore - ensures concurrency limit
                await _semaphore.WaitAsync(stoppingToken);
                // calls ProcessNextJob to handle one job at a time
                var task = ProcessNextJob(stoppingToken);
                tasks.Add(task);

                tasks.RemoveAll(t => t.IsCompleted);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in job worker main Loop");
                //Releases the System.Threading.SemaphoreSlim object a specified number of times.
                _semaphore.Release();
            }



        }
        await Task.WhenAll();
        _logger.LogInformation($"Job worker {_workerId} stopped");


    }

    private async Task ProcessNextJob(CancellationToken cancellationToken)
    {
        try
        {
            // Leases a Job - Tries to get the next available job from storage (LeaseNextJobAsync)

            var jobRecord = await _jobManager.LeaseNext(_workerId, _leaseDuration, cancellationToken: cancellationToken);
            // No jobs available
            if (jobRecord == null)
            {
                await Task.Delay(1000, cancellationToken);
                return;
            }
            // Executes the Job (execute) 
            _logger.LogInformation($"Worker {_workerId} processing job {jobRecord.Id} ");

            var result = await _jobExecutor.ExecuteAsync(jobRecord.Job, cancellationToken);
            if (result.Success)
            {
                // handle job success
                await HandleJobSuccess(jobRecord);
            }
            else
            {
                // handle job Failure
                await HandleJobFailure(jobRecord, result);
            }

        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task HandleJobSuccess(JobRecord jobRecord)
    {
        jobRecord.State = States.JobState.Succeeded;
        jobRecord.ProcessedAt = DateTime.UtcNow;
        jobRecord.LeaseId = null;
        jobRecord.LeaseExpiry = null;

        await _jobManager.Update(jobRecord);
        _logger.LogInformation($"Job {jobRecord.Id} completed completed successfully");
    }

    public async Task HandleJobFailure(JobRecord jobRecord, JobResult jobResult){

        jobRecord.RetryCount++;
        jobRecord.LastError = jobResult.ErrorMessage;
        
        jobRecord.LeaseId = null;
        jobRecord.LeaseExpiry = null;

        if(jobRecord.RetryCount >= jobRecord.MaxRetries){

            // move it to dead letter queue 
            // Prevents infinite retry loops and allows failed jobs to be inspected or
            // reprocessed manually later

            jobRecord.State = States.JobState.DeadLetter;
            _logger.LogWarning($"Job {jobRecord.Id} moved to dead letter queue...");
        }
        else{
            // Avoid overwhelming the system with immediate retries.
            var delay = TimeSpan.FromSeconds(Math.Pow(2, jobRecord.RetryCount - 1)*30);
            jobRecord.State  = States.JobState.Scheduled;
            jobRecord.ScheduledAt = DateTime.UtcNow.Add(delay);

             _logger.LogWarning("Job {JobId} scheduled for retry {RetryCount}/{MaxRetries} in {DelaySeconds}s", 
                    jobRecord.Id, jobRecord.RetryCount, jobRecord.MaxRetries, delay.TotalSeconds);
            }

            await _jobManager.Update(jobRecord);
        }

    }

