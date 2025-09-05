namespace Neon.Common;
public interface IJobScheduler
{
    Task<IEnumerable<JobRecord>> GetScheduledJobs(DateTime before, int maxCount, CancellationToken cancellationToken = default);
    Task<JobRecord?> LeaseNext(string workerId, TimeSpan leaseTime, string? queue = null, CancellationToken cancellationToken= default);
    Task ReleaseLease(string jobId, CancellationToken cancellationToken= default);
    Task<bool> TryExtendLease(string jobId, TimeSpan extension, CancellationToken cancellationToken= default);
}