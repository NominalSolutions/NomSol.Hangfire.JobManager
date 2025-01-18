namespace NomSol.Hangfire.JobManager.Core.Models
{
    public enum JobType
    {
        FireForget = 1,
        Recurring,
        Delayed
    }

    public enum ProcessType
    {
        Assembly = 1,
        ApiCall
    }
}
