using Hangfire.Dashboard;
using NomSol.Hangfire.JobManager.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NomSol.Hangfire.JobManager.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace NomSol.Hangfire.JobManager.Core.Dashboard
{
    public class UserSetupDispatcher : IDashboardDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserSetupDispatcher> _logger;

        public UserSetupDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<UserSetupDispatcher>>();
        }

        public async Task Dispatch(DashboardContext context)
        {
            try
            {
                if (!"POST".Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = 405;
                    return;
                }

                var repository = _serviceProvider.GetRequiredService<IHangfireJobManagerRepository>();
                
                // SECURITY: Only allow setup if NO users exist
                if (repository.GetAllUsers().Any())
                {
                    await RedirectToLogin(context, "Setup already completed.");
                    return;
                }

                async Task<string?> GetFormValue(string key)
                {
                    var values = await context.Request.GetFormValuesAsync(key);
                    return values != null && values.Count > 0 ? values[0] : null;
                }

                var username = await GetFormValue("UserName");
                var password = await GetFormValue("Password");
                var confirm = await GetFormValue("ConfirmPassword");

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    await RedirectWithErrorMessage(context, "Username and Password are required.");
                    return;
                }

                if (password != confirm)
                {
                    await RedirectWithErrorMessage(context, "Passwords do not match.");
                    return;
                }

                // Create admin
                var hashedPassword = PasswordHelper.HashPassword(password);
                repository.AddUser(username, "Admin", hashedPassword);

                // Redirect to login
                await RedirectToLogin(context, "Setup successful. Please login with your new account.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user setup.");
                await RedirectWithErrorMessage(context, "An internal error occurred during setup. Please check the logs.");
            }
        }

        private async Task RedirectWithErrorMessage(DashboardContext context, string message)
        {
            var httpContext = context.GetHttpContext();
            var redirectUrl = context.Request.PathBase + UserSetupPage.PageRoute + "?error=" + Uri.EscapeDataString(message);
            httpContext.Response.StatusCode = 302;
            httpContext.Response.Headers["Location"] = redirectUrl;
            await context.Response.WriteAsync($"Redirecting to {redirectUrl}...");
        }

        private async Task RedirectToLogin(DashboardContext context, string message)
        {
            var httpContext = context.GetHttpContext();
            var redirectUrl = context.Request.PathBase + UserLoginPage.PageRoute + "?error=" + Uri.EscapeDataString(message);
            httpContext.Response.StatusCode = 302;
            httpContext.Response.Headers["Location"] = redirectUrl;
            await context.Response.WriteAsync($"Redirecting to {redirectUrl}...");
        }
    }
}

