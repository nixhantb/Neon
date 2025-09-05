
using System.Collections.Concurrent;
using Neon.Common;

namespace Neon.Storage;

public class IMemoryJobStorage : IJobStorage, IJobScheduler, IJobQueue
{
    private readonly ConcurrentDictionary<string, JobRecord> _jobs = new();
    private readonly ILogger<IMemoryJobStorage> _logger;
    public IMemoryJobStorage(ILogger<IMemoryJobStorage> logger)
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
      public Task Delete(string jobId, CancellationToken cancellationToken = default)
    {
        _jobs.TryRemove(jobId, out _);
        return Task.CompletedTask;
    }
    public Task<IEnumerable<JobRecord>> GetScheduledJobs(DateTime before, int maxCount, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<JobRecord?> LeaseNext(string workerId, TimeSpan leaseTime, string? queue = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task ReleaseLease(string jobId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> TryExtendLease(string jobId, TimeSpan extension, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Update(JobRecord jobRecord, CancellationToken cancellationToken = default)
    {
        _jobs.TryUpdate(jobRecord.Id, jobRecord, _jobs[jobRecord.Id]);
        return Task.CompletedTask;

    }
}