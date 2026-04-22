using Hangfire.Dashboard;
using System.Reflection;

namespace NomSol.Hangfire.JobManager.Core.Dashboard
{
    /// <summary>
    /// Serves embedded resources (JS, CSS) from the assembly so there is no CDN dependency.
    /// </summary>
    public class EmbeddedResourceDispatcher : IDashboardDispatcher
    {
        private readonly Assembly _assembly;
        private readonly string _resourceName;
        private readonly string _contentType;

        public EmbeddedResourceDispatcher(Assembly assembly, string resourceName, string contentType)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            _resourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
            _contentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        }

        public async Task Dispatch(DashboardContext context)
        {
            context.Response.ContentType = _contentType;
            context.Response.SetExpire(DateTimeOffset.Now.AddYears(1));

            using var stream = _assembly.GetManifestResourceStream(_resourceName);
            if (stream == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            await stream.CopyToAsync(context.Response.Body);
        }
    }
}
