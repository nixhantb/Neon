using System.Linq.Expressions;

namespace Neon.Client
{
    public interface IJobClient
    {
        Task<string> EnqueueAsync(Expression<Action> methodCall);
        Task<string> EnqueueAsync(Expression<Func<Task>> methodCall);
        Task<string> EnqueueAsync<T>(Expression<Action<T>> methodCall);
        Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> methodCall);

        Task<string> ScheduleAsync(Expression<Action> methodCall, DateTime scheduledTime);
        Task<string> ScheduleAsync(Expression<Func<Task>> methodCall, DateTime scheduledTime);
        Task<string> ScheduleAsync<T>(Expression<Action<T>> methodCall, DateTime scheduledTime);
        Task<string> ScheduleAsync<T>(Expression<Func<T, Task>> methodCall, DateTime scheduledTime);

        Task<string> DelayAsync(Expression<Action> methodCall, TimeSpan delay);
        Task<string> DelayAsync(Expression<Func<Task>> methodcall, TimeSpan delay);
        Task<string> DelayAsync<T>(Expression<Action<T>> methodCall, TimeSpan delay);
        Task<string> DelayAsync<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);


        Task<string> RecurringAsync(string jobId, Expression<Action> methodCall, string cronExpression);
        Task<string> RecurringAsync(string jobId, Expression<Func<Task>> methodCall, string cronExpression);
        Task<string> RecurringAsync<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression);
        Task<string> RecurringAsync<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression);
    }
}