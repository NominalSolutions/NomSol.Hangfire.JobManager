using Hangfire.Dashboard;
using NomSol.Hangfire.JobManager.Core.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NomSol.Hangfire.JobManager.Core.Dashboard
{
    public class JobManagerUpdateDispatcher : IDashboardDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public JobManagerUpdateDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Dispatch(DashboardContext context)
        {
            var request = context.Request;
            var response = context.Response;

            var repository = _serviceProvider.GetRequiredService<IHangfireJobManagerRepository>();
            var schedulerService = _serviceProvider.GetRequiredService<ISchedulerService>();
            var authService = _serviceProvider.GetRequiredService<IJobManagerAuthorizationService>();

            if (!authService.IsAuthorized(context, "Editor"))
            {
                response.StatusCode = 403;
                return;
            }

            if (request.Method == "POST")
            {
                var jobIdValues = await request.GetFormValuesAsync("JobId");
                if (jobIdValues != null && jobIdValues.Count > 0 && long.TryParse(jobIdValues[0], out var jobId))
                {
                    var cronValues = await request.GetFormValuesAsync("CronExpression");
                    var cron = cronValues != null && cronValues.Count > 0 ? cronValues[0] : string.Empty;
                    
                    var argsValues = await request.GetFormValuesAsync("Arguments");
                    var args = argsValues != null && argsValues.Count > 0 ? argsValues[0] : string.Empty;
                    
                    var statusValues = await request.GetFormValuesAsync("Status");
                    var status = statusValues != null && statusValues.Count > 0 ? statusValues[0] : string.Empty;

                    // Basic validation for cron
                    if (string.IsNullOrWhiteSpace(cron) || (!cron.StartsWith("@") && cron.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 5))
                    {
                        var errorUrl = context.Request.PathBase + JobManagerPage.PageRoute + "?error=" + System.Net.WebUtility.UrlEncode("Invalid cron format. Must be a valid cron expression (5 or 6 fields) or a cron macro.");
                        await response.WriteAsync($"<script>window.location.href='{errorUrl}';</script>");
                        return;
                    }

                    var oldJob = repository.GetJobById(jobId);
                    if (oldJob != null)
                    {
                        var oldCron = oldJob.CronExpression;
                        var oldArgs = oldJob.Arguments;
                        var oldStatus = oldJob.Status;

                        try
                        {
                            repository.UpdateJob(jobId, cron, args, status);

                            // Ensure it loads the changes for this specific job or reloads all
                            schedulerService.ScheduleAllJobs(null); // Simple way to resync all recurring jobs
                        }
                        catch (Exception ex)
                        {
                            // Revert the DB change since validation/scheduling failed
                            repository.UpdateJob(jobId, oldCron, oldArgs, oldStatus);
                            
                            var errorMessage = ex.Message.Replace(" Please see the inner exception for details. (Parameter 'cronExpression')", "");
                            var errorUrl = context.Request.PathBase + JobManagerPage.PageRoute + "?error=" + System.Net.WebUtility.UrlEncode("An error occurred: " + errorMessage);
                            await response.WriteAsync($"<script>window.location.href='{errorUrl}';</script>");
                            return;
                        }
                    }
                }

                // Redirect back to page to prevent POST form resubmission
                int statusCode = 200;
                response.StatusCode = statusCode;
                var redirectUrl = context.Request.PathBase + JobManagerPage.PageRoute;
                await response.WriteAsync($"<script>window.location.href='{redirectUrl}';</script>");
                return;
            }

            return;
        }
    }
}
