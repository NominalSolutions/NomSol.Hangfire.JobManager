using System.Globalization;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Hangfire;
using Hangfire.Console;
using Hangfire.SqlServer;
using Microsoft.Extensions.Options;
using NomSol.Hangfire.JobManager.Core.Models;
using NomSol.Hangfire.JobManager.SqlServer;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NomSol.Hangfire.Dev.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Injecting DbConnectionFactories");

            // Register the ConnectionFactory as a scoped service with a specific connection string name
            var conStrings = configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();

            Log.Information("Adding Hangfire with SQL Server Storage option.");
            string hangfireSchema = "HangFireLocal";
            var storageOptions = new SqlServerStorageOptions
            {
                SchemaName = hangfireSchema,
                EnableHeavyMigrations = true
            };

            var nomsolJobManagerOptions = new NomSolJobManagerOptions
            {
                GenerateSampleJob = true
            };

            services.AddHangfireJobManagerBusinessServices();

            services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(conStrings.HangfireDatabase, storageOptions);
                config.UseConsole();
                config.UseJobManagerWithSql(serviceProvider: services.BuildServiceProvider(), services, sqlOptions: storageOptions, nomSolJobManagerOptions: nomsolJobManagerOptions);
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
                config.UseDefaultCulture(CultureInfo.GetCultureInfo("en-US"));
            });

            services.AddHangfireServer();
            return services;
        }

        public static IServiceCollection AddMinimalApiVersioning(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
        {
            string _applicationName = configuration.GetValue<string>("ApplicationSettings:ApplicationName") ?? throw new ArgumentException("Variable ApplicationName Missing");
            string _applicationVersion = configuration.GetValue<string>("ApplicationSettings:Version") ?? throw new ArgumentException("Variable Version Missing");

            services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1);
                o.ReportApiVersions = true;
                o.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("X-Version"),
                    new MediaTypeApiVersionReader("ver"));
            }).AddApiExplorer(
               options =>
               {
                   options.GroupNameFormat = "'v'V";
                   options.SubstituteApiVersionInUrl = true;
               });

            services.AddSwaggerGen();

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(serviceProvider =>
                new ConfigureSwaggerOptions(
                    serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>(),
                    environment,
                    _applicationName,
                    _applicationVersion));

            return services;
        }

        public static DashboardOptions _dashOptions_admin;

        public static IApplicationBuilder AddCustomSwagger(this IApplicationBuilder app, IConfiguration configuration, List<string> versions)
        {
            string? _applicationName = configuration.GetValue<string>("ApplicationSettings:ApplicationName");

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                foreach (var l in versions)
                {
                    c.SwaggerEndpoint($"/swagger/{l}/swagger.json", $"{_applicationName} {l}");
                }

                c.DocumentTitle = _applicationName;

                // Disabling options for model expansion and try out option
                c.DefaultModelsExpandDepth(-1);
                c.DefaultModelExpandDepth(-1);
                c.SupportedSubmitMethods();

                // For custom UI
                c.InjectStylesheet(@"/resources/css/custom-swagger.css");
                c.InjectJavascript(@"/resources/scripts/custom-script.js");
            });


            _dashOptions_admin = new DashboardOptions
            {
                AppPath = null,
                DashboardTitle = "Cash Jobs Services",
                StatsPollingInterval = 30000,
                DisplayStorageConnectionString = true,
                IsReadOnlyFunc = (dashboardContext) => false
            };
            app.UseHangfireDashboard("/admin", _dashOptions_admin);
            return app;
        }

        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            Log.Information("Injecting Business Services");

            //services.AddScoped<IService, ServService>();

            return services;
        }

    }
}
