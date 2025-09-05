
namespace Neon.Common;

public interface IJobStorage
{
    Task<JobRecord> GetJob(string JobId, CancellationToken cancellationToken = default);
    Task Update(JobRecord jobRecord, CancellationToken cancellationToken = default);
    Task Delete(string jobId, CancellationToken cancellationToken = default);
    
}