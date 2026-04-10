using NomSol.Hangfire.JobManager.Core.Models;
using NomSol.Hangfire.JobManager.Core.Models.Data.Tables;

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

        // User management (used when EnableUserAdmin is true)
        List<JobManagerUsers> GetAllUsers();
        JobManagerUsers? GetUserByUsername(string username);
        void AddUser(string username, string role, string? password = null);
        void UpdateUser(long userId, string username, string role, string? password = null);
        void DeleteUser(long userId);
    }
}
