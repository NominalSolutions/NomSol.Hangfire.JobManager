using Hangfire.Server;
using NomSol.Hangfire.JobManager.Core.Helpers;
using NomSol.Hangfire.JobManager.Core.Interfaces;
using NomSol.Hangfire.JobManager.Core.Models;
using NomSol.Hangfire.JobManager.SqlServer.Implementation.Business;

namespace NomSol.Hangfire.JobManager.SqlServer.Implementation
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IHangfireJobManagerRepository hangfireRepository;

        public SchedulerService(IHangfireJobManagerRepository _hangfireRepository)
        {
            this.hangfireRepository = _hangfireRepository;
        }

        public void ScheduleAllJobs(PerformContext dashLogger)
        {
            dashLogger.CustomLogger("Config: Scheduling Recurring Jobs", LogType.Information, false);
            SchedulerBusiness schedulerBusiness = new(hangfireRepository);
            schedulerBusiness.ScheduleJobsFromDB(null);
            dashLogger.CustomLogger("Config: Finished Scheduling Recurring Jobs", LogType.Information, false);
        }
    }
}
