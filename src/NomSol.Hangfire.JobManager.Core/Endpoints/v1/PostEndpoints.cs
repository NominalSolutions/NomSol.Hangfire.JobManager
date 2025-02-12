using Asp.Versioning;
using Asp.Versioning.Builder;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using NomSol.Hangfire.JobManager.Core.Endpoints.v1.Services;
using NomSol.Hangfire.JobManager.Core.Models;

namespace NomSol.Hangfire.JobManager.Core.Endpoints.v1
{
    public static class PostEndpoints
    {
        public static RouteGroupBuilder MapPost_v1(this IEndpointRouteBuilder routes, ApiVersionSet versionSet, ApiVersion version)
        {
            var group = routes.MapGroup("/jobmanager/v{version:apiVersion}/post");

            group.MapPost("/CreateFireForgetJob", async (IJobManagerServices _jobManagerService, FireForgetJobRequest request) =>
            {
                await _jobManagerService.AddFireForgetJob(request);

                RecurringJob.TriggerJob("UpdateRecurringJobs");

                return new BaseResponse();
            })
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(version)
            .WithOpenApi();

            return group;
        }
    }
}
