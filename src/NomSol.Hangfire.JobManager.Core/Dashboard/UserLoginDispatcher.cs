using Hangfire.Dashboard;
using NomSol.Hangfire.JobManager.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using NomSol.Hangfire.JobManager.Core.Helpers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace NomSol.Hangfire.JobManager.Core.Dashboard
{
    public class UserLoginDispatcher : IDashboardDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataProtector _protector;
        private readonly ILogger<UserLoginDispatcher> _logger;

        public UserLoginDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var dataProtectionProvider = _serviceProvider.GetRequiredService<IDataProtectionProvider>();
            _protector = dataProtectionProvider.CreateProtector("NomSol.Hangfire.JobManager.Auth");
            _logger = _serviceProvider.GetRequiredService<ILogger<UserLoginDispatcher>>();
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
                
                async Task<string?> GetFormValue(string key)
                {
                    var values = await context.Request.GetFormValuesAsync(key);
                    return values != null && values.Count > 0 ? values[0] : null;
                }

                var username = await GetFormValue("UserName");
                var password = await GetFormValue("Password");

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    await RedirectWithErrorMessage(context, "Username and Password are required.");
                    return;
                }

                // Check for setup: if NO users exist, redirect to setup
                if (!repository.GetAllUsers().Any())
                {
                    var setupUrl = context.Request.PathBase + UserSetupPage.PageRoute;
                    var httpContextForSetup = context.GetHttpContext();
                    httpContextForSetup.Response.StatusCode = 302;
                    httpContextForSetup.Response.Headers["Location"] = setupUrl;
                    return;
                }

                var user = repository.GetUserByUsername(username);
                if (user == null || string.IsNullOrEmpty(user.Password) || !PasswordHelper.VerifyPassword(password, user.Password)) 
                {
                    await RedirectWithErrorMessage(context, "Invalid username or password.");
                    return;
                }

                // Set secure (protected) cookie
                var protectedUsername = _protector.Protect(username);
                var httpContext = context.GetHttpContext();
                var path = httpContext.Request.PathBase.HasValue && !string.IsNullOrWhiteSpace(httpContext.Request.PathBase.Value)
                    ? httpContext.Request.PathBase.Value
                    : "/";

                httpContext.Response.Cookies.Append("NomSolJobManagerUser", protectedUsername, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = httpContext.Request.IsHttps,
                    Expires = DateTimeOffset.Now.AddDays(7),
                    Path = path,
                    SameSite = SameSiteMode.Lax
                });

                var redirectUrl = context.Request.PathBase + JobManagerPage.PageRoute;
                httpContext.Response.StatusCode = 302;
                httpContext.Response.Headers["Location"] = redirectUrl;
                await context.Response.WriteAsync($"Redirecting to {redirectUrl}...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");
                await RedirectWithErrorMessage(context, "An internal error occurred. Please try again later.");
            }
        }

        private async Task RedirectWithErrorMessage(DashboardContext context, string message)
        {
            var httpContext = context.GetHttpContext();
            var redirectUrl = context.Request.PathBase + UserLoginPage.PageRoute + "?error=" + Uri.EscapeDataString(message);
            httpContext.Response.StatusCode = 302;
            httpContext.Response.Headers["Location"] = redirectUrl;
            await context.Response.WriteAsync($"Redirecting to {redirectUrl}...");
        }
    }
}

