using Hangfire;
using Hangfire.Dashboard;
using Microsoft.Extensions.DependencyInjection;
using NomSol.Hangfire.JobManager.Core.Dashboard;
using NomSol.Hangfire.JobManager.Core.Interfaces;
using NomSol.Hangfire.JobManager.Core.Models;
using System.Reflection;

namespace NomSol.Hangfire.JobManager.Core
{
    public static class ServiceCollectionExtension
    {
        public static IGlobalConfiguration UseJobManager(this IGlobalConfiguration configuration, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var options = serviceProvider.GetService<NomSolJobManagerOptions>() ?? new NomSolJobManagerOptions();
            var repository = serviceProvider.GetRequiredService<IHangfireJobManagerRepository>();
            var authService = serviceProvider.GetRequiredService<IJobManagerAuthorizationService>();

            DashboardRoutes.Routes.AddRazorPage(JobManagerPage.PageRoute, x => new JobManagerPage(repository, authService, options));
            DashboardRoutes.Routes.Add(JobManagerPage.PageRoute + "/update", new JobManagerUpdateDispatcher(serviceProvider));

            if (options.UseLocalAuth)
            {
                DashboardRoutes.Routes.AddRazorPage(UserLoginPage.PageRoute, x => new UserLoginPage());
                DashboardRoutes.Routes.Add(UserLoginPage.PageRoute + "/submit", new UserLoginDispatcher(serviceProvider));
                
                DashboardRoutes.Routes.AddRazorPage(UserSetupPage.PageRoute, x => new UserSetupPage());
                DashboardRoutes.Routes.Add(UserSetupPage.PageRoute + "/submit", new UserSetupDispatcher(serviceProvider));
            }

            // Serve embedded cronstrue.min.js so there is no CDN dependency
            var assembly = typeof(JobManagerPage).Assembly;
            DashboardRoutes.Routes.Add("/jobmanager/js/cronstrue", new EmbeddedResourceDispatcher(assembly, "cronstrue.min.js", "application/javascript"));

            NavigationMenu.Items.Add(page => new MenuItem(JobManagerPage.Title, page.Url.To(JobManagerPage.PageRoute))
            {
                Active = page.RequestPath.StartsWith(JobManagerPage.PageRoute)
            });

            if (options.EnableUserAdmin)
            {
                DashboardRoutes.Routes.AddRazorPage(UserAdminPage.PageRoute, x => new UserAdminPage(repository, authService, options));
                DashboardRoutes.Routes.Add(UserAdminPage.PageRoute + "/update", new UserAdminUpdateDispatcher(serviceProvider));

                NavigationMenu.Items.Add(page => new MenuItem(UserAdminPage.Title, page.Url.To(UserAdminPage.PageRoute))
                {
                    Active = page.RequestPath.StartsWith(UserAdminPage.PageRoute)
                });
            }

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
