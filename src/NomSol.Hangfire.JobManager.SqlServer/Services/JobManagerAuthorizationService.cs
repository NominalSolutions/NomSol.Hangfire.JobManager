using Hangfire.Dashboard;
using NomSol.Hangfire.JobManager.Core.Interfaces;
using NomSol.Hangfire.JobManager.Core.Models;
using System.Linq;
using Microsoft.AspNetCore.DataProtection;
using System;

namespace NomSol.Hangfire.JobManager.SqlServer.Services
{
    /// <summary>
    /// Provides role-based authorization for the Job Manager dashboard.
    /// When EnableUserAdmin is false, all authorization checks pass (open access).
    /// </summary>
    public class JobManagerAuthorizationService : IJobManagerAuthorizationService
    {
        private readonly IHangfireJobManagerRepository _repository;
        private readonly NomSolJobManagerOptions _options;
        private readonly IDataProtector _protector;

        public bool IsEnabled => _options.EnableUserAdmin;

        public JobManagerAuthorizationService(
            IHangfireJobManagerRepository repository, 
            NomSolJobManagerOptions options,
            IDataProtectionProvider dataProtectionProvider)
        {
            _repository = repository;
            _options = options;
            _protector = dataProtectionProvider.CreateProtector("NomSol.Hangfire.JobManager.Auth");
        }

        public string? GetCurrentUsername(object dashboardContext)
        {
            if (!_options.EnableUserAdmin)
                return null;

            if (_options.UseLocalAuth && dashboardContext is DashboardContext context)
            {
                // Special check for setup: if NO users exist, we don't have a current user but we need redirect
                if (!_repository.GetAllUsers().Any())
                {
                    return "__SETUP_REQUIRED__";
                }

                var httpContext = context.GetHttpContext();
                if (httpContext.Request.Cookies.TryGetValue("NomSolJobManagerUser", out var protectedUsername))
                {
                    try
                    {
                        return _protector.Unprotect(protectedUsername);
                    }
                    catch (Exception)
                    {
                        // Decryption failed (invalid or tampered cookie)
                        return null;
                    }
                }
                return null;
            }

            if (_options.UserIdentityResolver == null)
                return null;

            return _options.UserIdentityResolver(dashboardContext);
        }

        public bool IsAuthorized(object dashboardContext, string requiredRole)
        {
            // If user admin is disabled, everyone has access
            if (!_options.EnableUserAdmin)
                return true;

            var username = GetCurrentUsername(dashboardContext);
            if (string.IsNullOrWhiteSpace(username))
                return false;

            var user = _repository.GetUserByUsername(username);
            if (user == null)
                return false;

            // Role Hierarchy: Admin > Editor > Viewer
            
            // 1. Admin access
            if (user.Role == "Admin")
                return true;

            // 2. Editor access
            if (requiredRole == "Editor")
            {
                return user.Role == "Editor";
            }

            // 3. Viewer access (anyone in the database is a viewer)
            if (requiredRole == "Viewer")
            {
                return true;
            }

            return false;
        }
    }
}

