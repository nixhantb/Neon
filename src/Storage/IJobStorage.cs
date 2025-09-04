
using Neon.Common;

public interface IJobStorage{
    Task<string> EnqueueAsync(JobRecord jobRecord, CancellationToken cancellationToken = default);
    Task<JobRecord> GetJobAsync(string JobId, CancellationToken cancellationToken = default);
    Task UpdateJobAsync(JobRecord jobRecord, CancellationToken cancellationToken = default);
    Task DeleteJobAsync(string jobId, CancellationToken cancellationToken = default);

    
}