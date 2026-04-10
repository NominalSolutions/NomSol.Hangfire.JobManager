using Hangfire;
using Hangfire.Dashboard;
using Microsoft.Extensions.DependencyInjection;
using NomSol.Hangfire.JobManager.Core.Dashboard;
using NomSol.Hangfire.JobManager.Core.Interfaces;
using System.Reflection;

namespace NomSol.Hangfire.JobManager.Core
{
    public static class ServiceCollectionExtension
    {
        public static IGlobalConfiguration UseJobManager(this IGlobalConfiguration configuration, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            DashboardRoutes.Routes.AddRazorPage(JobManagerPage.PageRoute, x => new JobManagerPage(serviceProvider.GetRequiredService<IHangfireJobManagerRepository>()));
            DashboardRoutes.Routes.Add(JobManagerPage.PageRoute + "/update", new JobManagerUpdateDispatcher(serviceProvider));

            // Serve embedded cronstrue.min.js so there is no CDN dependency
            var assembly = typeof(JobManagerPage).Assembly;
            DashboardRoutes.Routes.Add("/jobmanager/js/cronstrue", new EmbeddedResourceDispatcher(assembly, "cronstrue.min.js", "application/javascript"));

            NavigationMenu.Items.Add(page => new MenuItem(JobManagerPage.Title, page.Url.To(JobManagerPage.PageRoute))
            {
                Active = page.RequestPath.StartsWith(JobManagerPage.PageRoute)
            });

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
