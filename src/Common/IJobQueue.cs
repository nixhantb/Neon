

namespace Neon.Common;
public interface IJobQueue
{
    Task<string> Enqueue(JobRecord jobRecord, CancellationToken cancellationToken = default);
    Task<string> Dequeue(JobRecord jobRecord, CancellationToken cancellationToken = default);
    
}