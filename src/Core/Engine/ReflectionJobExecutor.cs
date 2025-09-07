using Neon.Common;

namespace Neon.Core.Engine;

public class ReflectionJobExecutor : IJobExecutor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReflectionJobExecutor> _logger;

    public ReflectionJobExecutor(IServiceProvider serviceProvider, ILogger<ReflectionJobExecutor> logger)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    /// <summary>
    /// Executes jobs dynamically using reflection.
    /// This executor can run any method on any class at runtime,
    /// without knowing the exact type or method when the code is compiled.
    /// 
    /// It logs the start of execution, resolves or creates the service instance,
    /// invokes the method (awaiting it if it’s asynchronous), and returns a JobResult
    /// indicating success or failure, including any exception information.
    /// This executor performs the following steps:
    /// 1. Logs the start of the job execution.
    /// 2. Resolves the service instance from the DI container or creates a new one if not found.
    /// 3. Invokes the specified method on the instance with provided arguments.
    /// 4. Awaits the method if it is asynchronous.
    /// 5. Returns a JobResult indicating success or failure.
    /// 6. Logs any exceptions that occur during execution and returns them in the JobResult.
    /// 
    /// This allows flexible execution of any method on any service type at runtime,
    /// without needing compile-time knowledge of the job's implementation.
    /// </summary>
    /// <param name="job"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<JobResult> ExecuteAsync(Job job, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Executing job {jobType}, {Method}", job.Type.Name, job.Method.Name);


            var instance = _serviceProvider.GetService(job.Type) ?? Activator.CreateInstance(job.Type);
            if (instance == null)
            {
                throw new InvalidOperationException($"Could not create instance of {job.Type}");
            }

            var result = job.Method.Invoke(instance, job.Args.ToArray());

            if (result is Task task)
            {
                await task;
            }

            return new JobResult { Success = true };

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job execution failed for {JobType}.{Method}", job.Type.Name, job.Method.Name);
            return new JobResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Exception = ex
            };
        }
    }
}