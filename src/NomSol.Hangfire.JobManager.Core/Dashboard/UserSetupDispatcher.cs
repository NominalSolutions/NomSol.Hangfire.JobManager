using Hangfire.Dashboard;
using NomSol.Hangfire.JobManager.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NomSol.Hangfire.JobManager.Core.Helpers;
using System.Linq;

namespace NomSol.Hangfire.JobManager.Core.Dashboard
{
    public class UserSetupDispatcher : IDashboardDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public UserSetupDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Dispatch(DashboardContext context)
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
                RedirectToLogin(context, "Setup already completed.");
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
                RedirectWithErrorMessage(context, "Username and Password are required.");
                return;
            }

            if (password != confirm)
            {
                RedirectWithErrorMessage(context, "Passwords do not match.");
                return;
            }

            // Create admin
            var hashedPassword = PasswordHelper.HashPassword(password);
            repository.AddUser(username, "Admin", hashedPassword);

            // Redirect to login
            RedirectToLogin(context, "Setup successful. Please login with your new account.");
        }

        private void RedirectWithErrorMessage(DashboardContext context, string message)
        {
            var httpContext = context.GetHttpContext();
            var redirectUrl = context.Request.PathBase + UserSetupPage.PageRoute + "?error=" + Uri.EscapeDataString(message);
            httpContext.Response.StatusCode = 302;
            httpContext.Response.Headers["Location"] = redirectUrl;
            context.Response.WriteAsync($"Redirecting with error to {redirectUrl}...");
        }

        private void RedirectToLogin(DashboardContext context, string message)
        {
            var httpContext = context.GetHttpContext();
            var redirectUrl = context.Request.PathBase + UserLoginPage.PageRoute + "?error=" + Uri.EscapeDataString(message);
            httpContext.Response.StatusCode = 302;
            httpContext.Response.Headers["Location"] = redirectUrl;
            context.Response.WriteAsync($"Redirecting to {redirectUrl}...");
        }
    }
}
