
using System.Collections.Concurrent;
using Neon.Common;
using Neon.Core.States;

namespace Neon.Storage;

public class MemoryJobStorage : IJobManager
{
    private readonly ConcurrentDictionary<string, JobRecord> _jobs = new();
    private readonly ILogger<MemoryJobStorage> _logger;
    public MemoryJobStorage(ILogger<MemoryJobStorage> logger)
    {
        _logger = logger;
    }

    public Task<string> Enqueue(JobRecord jobRecord, CancellationToken cancellationToken = default)
    {
        jobRecord.Id = Guid.NewGuid().ToString();
        jobRecord.CreatedAt = DateTime.UtcNow;
        _jobs.TryAdd(jobRecord.Id, jobRecord);
        _logger.LogDebug("Job {JobId} enqueued to Queue {Queue} ", jobRecord.Id, jobRecord.Job.Queue);
        return Task.FromResult(jobRecord.Id);

    }
    public Task<string> Dequeue(JobRecord jobRecord, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<JobRecord> GetJob(string JobId, CancellationToken cancellationToken = default)
    {
        _jobs.TryGetValue(JobId, out var job);
        return Task.FromResult(job);
    }
    /// <summary>
    /// Delete the Job
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task Delete(string jobId, CancellationToken cancellationToken = default)
    {
        _jobs.TryRemove(jobId, out _);
        return Task.CompletedTask;
    }
    /// <summary>
    /// 1. Get all jobs
    /// 2. Filter only Enqueued or Scheduled
    /// 3. Filter only jobs due before prevTime
    /// 4. Filter out leased jobs
    /// 5. Sort oldest first
    /// 6. Limit by maxCount
    /// </summary>
    /// <param name="prevTime"></param>
    /// <param name="maxCount"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IEnumerable<JobRecord>> GetScheduledJobs(DateTime prevTime, int maxCount, CancellationToken cancellationToken = default)
    {

        var jobs = _jobs.Values
        .Where(j =>
            (j.State == JobState.Enqueued || j.State == JobState.Scheduled) &&   // only jobs ready or scheduled
            (j.ScheduledAt == null || j.ScheduledAt <= prevTime) &&              // scheduled time has arrived
            (j.LeaseId == null || j.LeaseExpiry < DateTime.UtcNow))              // not currently leased
        .OrderBy(j => j.CreatedAt)                                               // oldest first
        .Take(maxCount);                                                         // cap the results

        return Task.FromResult(jobs);
    }
    /// <summary>
    /// Check if the job is enqueued
    /// Check if the job is ready to run
    /// Check if the job is not already leased by another worker
    /// If no queue is passed, accept any queue.
    /// </summary>
    /// <param name="workerId"></param>
    /// <param name="leaseTime"></param>
    /// <param name="queue"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<JobRecord?> LeaseNext(string workerId, TimeSpan leaseTime, string? queue = null, CancellationToken cancellationToken = default)
    {
        var currentTime = DateTime.UtcNow;

        var job = _jobs.Values

                .Where(jb => jb.State == JobState.Enqueued

                        && (jb.ScheduledAt == null || jb.ScheduledAt <= currentTime)

                        && (jb.LeaseId == null || jb.LeaseExpiry < currentTime)

                        && (queue == null || jb.Job.Queue == queue))

                       .OrderBy(j => j.CreatedAt).FirstOrDefault();
                
                if(job!=null){

                    job.Id = workerId;
                    job.LeaseExpiry = currentTime.Add(leaseTime);
                    job.State = JobState.Processing;
                    _logger.LogDebug("Job {JobId} leased to worker {WorkerId}", job.Id, workerId);
                };

                return Task.FromResult(job);
    }
    /// <summary>
    /// Look up the job by its ID in the job collection.
    /// If the job exists: Clear the lease information (remove worker assignment and expiry time).
    /// If the job is currently marked as Processing,
    /// set it back to Enqueued so it can be picked up again.
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    public Task ReleaseLease(string jobId, CancellationToken cancellationToken = default)
    {
        var jobs = _jobs.TryGetValue(jobId, out var job);
        if(jobs){
            job!.LeaseId = null;
            job.LeaseExpiry = null;

            if(job.State == JobState.Processing){
                job.State = JobState.Enqueued;
            }
        }
        return Task.FromResult(jobs);
    }
    /// <summary>
    /// Extending the lease with extension time
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="extension"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    public Task<bool> TryExtendLease(string jobId, TimeSpan extension, CancellationToken cancellationToken = default)
    {
        var jobs = _jobs.TryGetValue(jobId, out var job);
        

        if(jobs && job!.LeaseExpiry.HasValue){
            
            job.LeaseExpiry = job.LeaseExpiry.Value.Add(extension);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
    /// <summary>
    /// Update the Job
    /// </summary>
    /// <param name="jobRecord"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task Update(JobRecord jobRecord, CancellationToken cancellationToken = default)
    {
        _jobs.TryUpdate(jobRecord.Id, jobRecord, _jobs[jobRecord.Id]);
        return Task.CompletedTask;

    }

    public Task<Dictionary<JobState, int>> GetJobCountsByState(CancellationToken cancellationToken = default)
    {
        var jobCounts = _jobs.Values.GroupBy(j => j.State)
        .ToDictionary(g => g.Key, g => g.Count());

        return Task.FromResult(jobCounts);
    }

    public Task<Dictionary<string, int>> GetQueueLength(CancellationToken cancellationToken = default)
    {
        var queueLengths = _jobs.Values.Where(j => j.State == JobState.Enqueued || j.State == JobState.Scheduled)
                        .GroupBy(q => q.Job.Queue)
                        .ToDictionary(g => g.Key, q => q.Count());
        return Task.FromResult(queueLengths);
    }
}
