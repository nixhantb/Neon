using Neon.Common;

namespace Neon.Core.Engine
{
    /// <summary>
    /// The ReflectionJobExecutor implements this and it is responsible for executing a Job object dynamically using reflection.
    /// It takes the Job (which contains a type, method, and arguments) and runs it, 
    /// handling both synchronous and asynchronous methods. It also logs execution and captures errors.
    /// </summary>
    public interface IJobExecutor
    {
        Task<JobResult> ExecuteAsync(Job job, CancellationToken cancellationToken = default);
    }
}