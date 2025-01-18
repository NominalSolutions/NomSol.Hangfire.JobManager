using Hangfire;
using NomSol.Hangfire.JobManager.Core.Interfaces;

namespace NomSol.Hangfire.JobManager.Core
{
    public static class ServiceCollectionExtension
    {
        public static IGlobalConfiguration UseJobManager(this IGlobalConfiguration configuration, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            StartHangfireJobs(serviceProvider);

            return configuration;
        }

        private static void StartHangfireJobs(this IServiceProvider serviceProvider)
        {
            var jobSchedulerService = serviceProvider.GetService(typeof(ISchedulerService)) as ISchedulerService;
            jobSchedulerService?.ScheduleAllJobs(null);
        }
    }
}
