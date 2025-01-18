using Asp.Versioning.Builder;
using NomSol.Hangfire.Dev.Endpoints.v1;

namespace NomSol.Hangfire.Dev.Extensions
{
    public static class EndpointConfigurationExtension
    {
        public static void MapEndpointsForVersion(this WebApplication app, ApiVersionSet versionSet, List<CustomApiVersionModel> version)
        {
            app.MapGet_v1(versionSet, version[0].Version).RequireAuthorization();
        }
    }
}
