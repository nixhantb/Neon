using Neon.Client;
using Neon.Core.Engine;
using Neon.Storage;

namespace Neon.Samples;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddJobFlow(this IServiceCollection services)
    {
        // core services
        services.AddSingleton<IJobManager, MemoryJobStorage>();
        services.AddSingleton<IJobExecutor, ReflectionJobExecutor>();
        services.AddSingleton<IJobClient, JobClient>();

        // Background services
        services.AddHostedService<JobScheduler>();
        services.AddHostedService<JobWorker>();

        // Sample Job Service
        services.AddTransient<EmailService>();
        services.AddTransient<DataProcessingService>();

        return services;
    }
    
}