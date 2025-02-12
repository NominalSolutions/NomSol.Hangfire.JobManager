using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using NomSol.Hangfire.JobManager.Core.Endpoints.v1;
using NomSol.Hangfire.JobManager.Core.Models;

namespace NomSol.Hangfire.JobManager.Core
{
    public static class EndpointConfigurationExtension
    {
        // Add before app.Run();
        public static void EnableNomSolJobManagerEndpoints(this WebApplication app, bool RequireAuthorization = true)
        {
            List<CustomApiVersionModel> apiVersions = Helpers.Helpers.ReturnApiVersions();

            ApiVersionSet vSet = app.NewApiVersionSet().HasApiVersion(apiVersions[0].Version).Build();

            if (RequireAuthorization)
            {
                app.MapGet_v1(vSet, apiVersions[0].Version).RequireAuthorization();
                app.MapPost_v1(vSet, apiVersions[0].Version).RequireAuthorization();
            }
            else
            {
                app.MapGet_v1(vSet, apiVersions[0].Version);
                app.MapPost_v1(vSet, apiVersions[0].Version);
            };
        }
    }
}
