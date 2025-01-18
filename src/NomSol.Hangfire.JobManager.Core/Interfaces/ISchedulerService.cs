using Hangfire.Server;

namespace NomSol.Hangfire.JobManager.Core.Interfaces
{
    public interface ISchedulerService
    {
        void ScheduleAllJobs(PerformContext dashLogger);
    }
}
