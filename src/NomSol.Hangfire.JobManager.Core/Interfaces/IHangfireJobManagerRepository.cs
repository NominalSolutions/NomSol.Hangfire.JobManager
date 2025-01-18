namespace NomSol.Hangfire.JobManager.Core.Interfaces
{
    public interface IHangfireJobManagerRepository
    {
        List<Models.Data.Tables.JobManager> GetAllJobs();
        void MarkJobComplete(long jobId, string jobName);
    }
}
