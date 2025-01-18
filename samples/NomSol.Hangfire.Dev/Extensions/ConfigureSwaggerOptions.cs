using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NomSol.Hangfire.Dev.Extensions
{
    public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IHostEnvironment hostingEnvironment, string applicationName, string release) : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider = provider;
        private readonly IHostEnvironment _hostingEnvironment = hostingEnvironment;
        private readonly string _applicationName = applicationName;
        private readonly string _release = release;

        public void Configure(SwaggerGenOptions options)
        {
            string _appDescription = $"Environment: {_hostingEnvironment.EnvironmentName}" + (string.IsNullOrWhiteSpace(_release) ? string.Empty : $"  {Environment.NewLine}Release: {_release}");

            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Title = $"{_applicationName}",
                    Version = description.ApiVersion.ToString(),
                    Description = _appDescription,
                    TermsOfService = new Uri("https://www.nominal.solutions"),
                    Contact = new OpenApiContact
                    {
                        Email = "jermy@jermy.dev",
                        Name = "Development Team"
                    },
                    License = new OpenApiLicense
                    {
                        Name = $"Copyright {DateTime.Now.Year}, Nominal Solutions LLC. All rights reserved.",
                    }
                });
            }
        }
    }
}
