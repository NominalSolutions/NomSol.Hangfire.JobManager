using Asp.Versioning;
using Asp.Versioning.Builder;

namespace NomSol.Hangfire.Dev.Endpoints.v1
{
    public static class GetEndpoints
    {
        public static RouteGroupBuilder MapGet_v1(this IEndpointRouteBuilder routes, ApiVersionSet versionSet, ApiVersion version)
        {
            var group = routes.MapGroup("/v{version:apiVersion}/get");

            group.MapGet("/helloworld", () =>
            {
                return "Hello World!";
            })
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(version)
            .WithOpenApi();

            return group;
        }
    }
}
