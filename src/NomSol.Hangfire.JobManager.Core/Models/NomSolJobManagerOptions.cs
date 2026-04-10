using System;

namespace NomSol.Hangfire.JobManager.Core.Models
{
    public class NomSolJobManagerOptions
    {
        public bool GenerateSampleJob { get; set; } = false;

        /// <summary>
        /// Whether user administration is enabled. This is automatically true if a <see cref="UserIdentityResolver"/> is provided or if <see cref="UseLocalAuth"/> is enabled.
        /// </summary>
        public bool EnableUserAdmin => UserIdentityResolver != null || UseLocalAuth;

        /// <summary>
        /// When true, the Job Manager will use its own internal user database for authentication (username/password).
        /// This will add a login page to the dashboard.
        /// </summary>
        public bool UseLocalAuth { get; set; } = false;

        /// <summary>
        /// Resolves the current user's identity from the DashboardContext.
        /// Example: ctx => (ctx as DashboardContext)?.GetHttpContext()?.User?.Identity?.Name
        /// </summary>
        public Func<object, string?>? UserIdentityResolver { get; set; }
    }
}
