using NomSol.Hangfire.JobManager.Core.Models;

namespace NomSol.Hangfire.JobManager.Core.Interfaces
{
    public interface IHangfireJobManagerRepository
    {
        List<Models.Data.Tables.JobManager> GetAllJobs();
        List<Models.Data.Tables.JobManager> GetAllRecurringJobs();
        Models.Data.Tables.JobManager? GetJobById(long jobId);
        void UpdateJob(long jobId, string cronExpression, string arguments, string status);
        void MarkJobComplete(long jobId, string jobName);
        Task AddFireForgetJob(FireForgetJobRequest request);
    }
}
