using Neon.Common;

namespace Neon.Core.Engine;

public class JobScheduler : BackgroundService
{
    private readonly ILogger<JobScheduler> _logger;
    private readonly IJobManager _jobStorage;

    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(5);

    public JobScheduler(IJobManager jobStorage, ILogger<JobScheduler> logger)
    {
        _logger = logger;
        _jobStorage = jobStorage;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Job Scheduler started");


        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessScheduledJobs(stoppingToken);
                await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in job scheduler");
                await Task.Delay(_pollInterval, stoppingToken);
            }
        }
        _logger.LogInformation("Job Scheduler stopped");

    }

    private async Task ProcessScheduledJobs(CancellationToken cancellationToken)
    {
        var scheduledJobs = await _jobStorage.GetScheduledJobs(DateTime.UtcNow, 5, cancellationToken);

        foreach (var jobRecord in scheduledJobs)
        {
            if (!string.IsNullOrEmpty(jobRecord.CronExpression))
            {
                // handle recurring job

                await HandleRecurringJob(jobRecord, cancellationToken);
            }
            else
            {
                // markjobasready
                await MarkJobAsReady(jobRecord, cancellationToken);
            }
        }
    }

    private async Task MarkJobAsReady(JobRecord jobRecord, CancellationToken cancellationToken)
    {
        jobRecord.State = States.JobState.Enqueued;
        jobRecord.ScheduledAt = null;
        await _jobStorage.Update(jobRecord, cancellationToken);
    }
    private async Task HandleRecurringJob(JobRecord jobRecord, CancellationToken cancellationToken)
    {


        var nextRun = CalculateNextRun(jobRecord.CronExpression, jobRecord.ProcessedAt ?? jobRecord.CreatedAt);

        if (nextRun <= DateTime.UtcNow)
        {
            var newJobRecord = new JobRecord
            {
                Job = new Job
                {
                    Type = jobRecord.Job.Type,
                    Method = jobRecord.Job.Method,
                    Args = jobRecord.Job.Args,
                    Queue = jobRecord.Job.Queue
                },
                MaxRetries = jobRecord.MaxRetries
            };

            await _jobStorage.Enqueue(newJobRecord, cancellationToken);

            jobRecord.NextRun = CalculateNextRun(jobRecord.CronExpression, DateTime.UtcNow);
            jobRecord.ProcessedAt = DateTime.UtcNow;
            await _jobStorage.Update(jobRecord, cancellationToken);

             _logger.LogDebug("Recurring job {JobId} scheduled next instance", jobRecord.Id);
        }

    }

    private DateTime CalculateNextRun(string CronExpression, DateTime from){
        return from.AddMinutes(5);
    }


    
    
}