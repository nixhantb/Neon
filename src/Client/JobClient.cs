using System.Linq.Expressions;
using Neon.Common;

namespace Neon.Client;

public class JobClient : IJobClient

{
    private readonly IJobManager _jobManager;
    private readonly ILogger<JobClient> _logger;

    public JobClient(IJobManager jobManager, ILogger<JobClient> logger)
    {
        _jobManager = jobManager;
        _logger = logger;
    }

    public async Task<string> DelayAsync(Expression<Action> methodCall, TimeSpan delay)
    {
        return await ScheduleAsync(methodCall, DateTime.UtcNow.Add(delay));
    }

    public async Task<string> DelayAsync(Expression<Func<Task>> methodCall, TimeSpan delay)
    {
        return await ScheduleAsync(methodCall, DateTime.UtcNow.Add(delay));
    }

    public async Task<string> DelayAsync<T>(Expression<Action<T>> methodCall, TimeSpan delay)
    {
        return await ScheduleAsync(methodCall, DateTime.UtcNow.Add(delay));
    }

    public async Task<string> DelayAsync<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
    {
        return await ScheduleAsync(methodCall, DateTime.UtcNow.Add(delay));
    }

    public async Task<string> EnqueueAsync(Expression<Action> methodCall)
    {
        var job = Job.FromExpression(methodCall);
        return await EnqueueJobInternal(job);
    }

    public async Task<string> EnqueueAsync(Expression<Func<Task>> methodCall)
    {
        var job = Job.FromExpression(methodCall);
        return await EnqueueJobInternal(job);
    }

    public async Task<string> EnqueueAsync<T>(Expression<Action<T>> methodCall)
    {
        var job = Job.FromExpression(methodCall);
        return await EnqueueJobInternal(job);
    }

    public async Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> methodCall)
    {
        var job = Job.FromExpression(methodCall);
        return await EnqueueJobInternal(job);
    }

    public async Task<string> RecurringAsync(string jobId, Expression<Action> methodCall, string cronExpression)
    {
        var job = Job.FromExpression(methodCall);
        return await RecurringJobInternal(jobId, job, cronExpression);
    }

    public async Task<string> RecurringAsync(string jobId, Expression<Func<Task>> methodCall, string cronExpression)
    {
        var job = Job.FromExpression(methodCall);
        return await RecurringJobInternal(jobId, job, cronExpression);
    }

    public async Task<string> RecurringAsync<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression)
    {
        var job = Job.FromExpression(methodCall);
        return await RecurringJobInternal(jobId, job, cronExpression);
    }

    public async Task<string> RecurringAsync<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression)
    {
        var job = Job.FromExpression(methodCall);
        return await RecurringJobInternal(jobId, job, cronExpression);
    }

    public async Task<string> ScheduleAsync(Expression<Action> methodCall, DateTime scheduledTime)
    {
        var job = Job.FromExpression(methodCall);
        return await ScheduleJobInternal(job, scheduledTime);
    }

    public async Task<string> ScheduleAsync(Expression<Func<Task>> methodCall, DateTime scheduledTime)
    {
        var job = Job.FromExpression(methodCall);
        return await ScheduleJobInternal(job, scheduledTime);
    }

    public async Task<string> ScheduleAsync<T>(Expression<Action<T>> methodCall, DateTime scheduledTime)
    {
        var job = Job.FromExpression(methodCall);
        return await ScheduleJobInternal(job, scheduledTime);
    }

    public async Task<string> ScheduleAsync<T>(Expression<Func<T, Task>> methodCall, DateTime scheduledTime)
    {
        var job = Job.FromExpression(methodCall);
        return await ScheduleJobInternal(job, scheduledTime);
    }

    private async Task<string> EnqueueJobInternal(Job job)
    {
        var jobRecord = new JobRecord
        {
            Job = job

        };

        var jobId = await _jobManager.Enqueue(jobRecord);
        _logger.LogInformation("Job {JobId} enqueued for immediate execution", jobId);

        return jobId;

    }

    private async Task<string> ScheduleJobInternal(Job job, DateTime scheduledTime)
    {
        var jobRecord = new JobRecord
        {
            Job = job,
            State = Core.States.JobState.Scheduled,
            ScheduledAt = scheduledTime
        };

        var jobId = await _jobManager.Enqueue(jobRecord);
        _logger.LogInformation("Job {JobId} scheduled for {ScheduledTime}", jobId, scheduledTime);

        return jobId;
    }

    private async Task<string> RecurringJobInternal(string jobId, Job job, string cronExpression)
    {

        var jobRecord = new JobRecord
        {
            Id = jobId,
            Job = job,
            CronExpression = cronExpression,
            State = Core.States.JobState.Scheduled,
            NextRun = DateTime.UtcNow
        };

         await _jobManager.Enqueue(jobRecord);
            _logger.LogInformation("Recurring job {JobId} registered with cron {CronExpression}", jobId, cronExpression);
            
            return jobId;
    }

}