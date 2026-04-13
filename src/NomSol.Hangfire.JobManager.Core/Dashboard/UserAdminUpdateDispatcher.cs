using Hangfire.Dashboard;
using NomSol.Hangfire.JobManager.Core.Interfaces;
using NomSol.Hangfire.JobManager.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NomSol.Hangfire.JobManager.Core.Dashboard
{
    public class UserAdminUpdateDispatcher : IDashboardDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserAdminUpdateDispatcher> _logger;

        public UserAdminUpdateDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<UserAdminUpdateDispatcher>>();
        }

        public async Task Dispatch(DashboardContext context)
        {
            var repository = _serviceProvider.GetRequiredService<IHangfireJobManagerRepository>();
            var authService = _serviceProvider.GetRequiredService<IJobManagerAuthorizationService>();

            // Only Admins can manage users
            if (!authService.IsAuthorized(context, "Admin"))
            {
                context.Response.StatusCode = 403;
                return;
            }

            if (!"POST".Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 405;
                return;
            }

            async Task<string?> GetFormValue(string key)
            {
                var values = await context.Request.GetFormValuesAsync(key);
                return values != null && values.Count > 0 ? values[0] : null;
            }

            var action = await GetFormValue("Action");
            var username = await GetFormValue("Username");
            var role = await GetFormValue("Role");
            var password = await GetFormValue("Password");
            var userIdStr = await GetFormValue("UserId");

            string? error = null;

            try
            {
                if ("Add".Equals(action, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(role))
                    {
                        error = "Username and Role are required.";
                    }
                    else
                    {
                        var hashedPassword = string.IsNullOrWhiteSpace(password) ? null : PasswordHelper.HashPassword(password);
                        repository.AddUser(username!, role!, hashedPassword);
                    }
                }
                else if ("Update".Equals(action, StringComparison.OrdinalIgnoreCase))
                {
                    if (long.TryParse(userIdStr, out var id) && !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(role))
                    {
                        var hashedPassword = string.IsNullOrWhiteSpace(password) ? null : PasswordHelper.HashPassword(password);
                        repository.UpdateUser(id, username!, role!, hashedPassword);
                    }
                    else
                    {
                        error = "Invalid user data for update.";
                    }
                }
                else if ("Delete".Equals(action, StringComparison.OrdinalIgnoreCase))
                {
                    if (long.TryParse(userIdStr, out var id))
                    {
                        repository.DeleteUser(id);
                    }
                    else
                    {
                        error = "Invalid user ID for deletion.";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user admin update.");
                error = "An internal error occurred during the update. Please check the logs.";
            }

            var redirectUrl = context.Request.PathBase + context.Request.Path.Replace("/update", "");
            if (!string.IsNullOrEmpty(error))
            {
                redirectUrl += "?error=" + Uri.EscapeDataString(error);
            }

            var httpContext = context.GetHttpContext();
            httpContext.Response.StatusCode = 302;
            httpContext.Response.Headers["Location"] = redirectUrl;
            await context.Response.WriteAsync($"Redirecting to {redirectUrl}...");
        }
    }
}

