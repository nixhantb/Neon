using Neon.Client;
using Neon.Samples;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Job job1 = Job.FromExpression(() => new Worker().DoWork("hello", 5), "default");
// Console.WriteLine($"Job1 -> Type: {job1.Type}, Method: {job1.Method.Name}, Args: {string.Join(", ", job1.Args)}");
// Build and configure the generic host
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddJobFlow();
    })
    .ConfigureLogging(builder =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

var jobClient = host.Services.GetRequiredService<IJobClient>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();


var hostTask = host.RunAsync();




// 1. Immediate jobs using expression trees 
var immediateJobId = await jobClient.EnqueueAsync<EmailService>(x => x.SendWelcomeEmail("john@example.com", "John Doe"));
logger.LogInformation("Enqueued immediate job: {JobId}", immediateJobId);


var notificationJobId = await jobClient.EnqueueAsync<EmailService>(x => x.SendNotification("nix124@fs", "Welcome notification"));
logger.LogInformation("Enqueued notification job: {JobId}", notificationJobId);


var delayedJobId = await jobClient.DelayAsync<DataProcessingService>(x => x.ProcessUserDataAsync(3242), TimeSpan.FromSeconds(10));
logger.LogInformation("Enqueued delayed job: {JobId}", delayedJobId);

var recurringJobId = await jobClient.RecurringAsync(
          "cleanup-temp-files",
          () => new DataProcessingService(null!).CleanupTempFilesAsync(),
          "0 */1 * * * *"); // Every 1 minutes
logger.LogInformation("Registered recurring job: {JobId}", recurringJobId);

var bulkJobs = new List<Task<string>>();
for (int i = 0; i < 100; i++)
{
    int subscriptionId = 1000 + i; // Capture loop variable
    var task = jobClient.EnqueueAsync<EmailService>(x => x.SendNewsLetter(subscriptionId));
    bulkJobs.Add(task);
}
await Task.WhenAll(bulkJobs);
logger.LogInformation("Enqueued {Count} newsletter jobs using expression trees", bulkJobs.Count);
app.Run();
