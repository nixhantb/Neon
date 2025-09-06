using Neon.Core.States;

namespace Neon.Common;

public interface IJobMonitoring
{
    Task<Dictionary<JobState, int>> GetJobCountsByState(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetQueueLength(CancellationToken cancellationToken = default);
}