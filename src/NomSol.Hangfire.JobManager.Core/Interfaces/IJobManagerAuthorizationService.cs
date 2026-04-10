namespace NomSol.Hangfire.JobManager.Core.Interfaces
{
    /// <summary>
    /// Provides role-based authorization for the Job Manager dashboard.
    /// When user admin is disabled, all checks return true (open access).
    /// </summary>
    public interface IJobManagerAuthorizationService
    {
        /// <summary>
        /// Checks if the current user has the specified role (or higher).
        /// Admin > Editor.
        /// </summary>
        bool IsAuthorized(object dashboardContext, string requiredRole);

        /// <summary>
        /// Resolves the current user's username from the dashboard context.
        /// Returns null if no identity resolver is configured or user is anonymous.
        /// </summary>
        string? GetCurrentUsername(object dashboardContext);

        /// <summary>
        /// Whether user admin is enabled.
        /// </summary>
        bool IsEnabled { get; }
    }
}
