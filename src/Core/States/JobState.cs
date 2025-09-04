namespace Neon.Core.States
{
    /// <summary>
    /// Extension area in the v2, seperate classes and more validations for 
    /// each of the states defined below.
    /// </summary>
    public enum JobState
    {
        Enqueued,
        Processing,
        Succeeded,
        Failed,
        Scheduled,
        Deleted,
        DeadLetter
    }
}