

using Neon.Core.States;

namespace Neon.Common
{
    public class JobRecord
    {

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Job Job { get; set; } = null!;
    
        public JobState State { get; set; } = JobState.Enqueued;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ScheduledAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 10;
        public string? CronExpression { get; set; }
        public string? LastError { get; set; }
        public DateTime? NextRun { get; set; }
        /// <summary>
        /// A lease is a temporary “lock” on a job that a worker takes when it starts processing the job.
        /// It prevents other workers from picking up the same job at the same time.
        /// </summary>
        public string? LeaseId { get; set; }
        public DateTime? LeaseExpiry { get; set; }

    }

    public class JobResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }

    }
}