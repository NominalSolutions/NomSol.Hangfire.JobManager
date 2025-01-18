using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NomSol.Hangfire.JobManager.Core;
using NomSol.Hangfire.JobManager.Core.Interfaces;
using NomSol.Hangfire.JobManager.Core.Models;
using NomSol.Hangfire.JobManager.SqlServer.Data;
using NomSol.Hangfire.JobManager.SqlServer.Data.Context;
using NomSol.Hangfire.JobManager.SqlServer.Data.Repository;
using NomSol.Hangfire.JobManager.SqlServer.Implementation;
using System;

namespace NomSol.Hangfire.JobManager.SqlServer
{
    public static class ServiceCollectionExtension
    {
        private static string connectionString = "";
        public static string _schemaName = "Hangfire";
        private static MonitoringApiHelper GetMonitoringApi(JobStorage jobStorage)
        {
            return new MonitoringApiHelper(jobStorage.GetMonitoringApi());
        }

        public static IGlobalConfiguration UseJobManagerWithSql(this IGlobalConfiguration configuration, IServiceProvider serviceProvider, IServiceCollection services, SqlServerStorageOptions? sqlOptions = null, NomSolJobManagerOptions? nomSolJobManagerOptions = null)
        {
            sqlOptions ??= new SqlServerStorageOptions();
            nomSolJobManagerOptions ??= new NomSolJobManagerOptions();

            // Get the current JobStorage
            var jobStorage = JobStorage.Current;

            if (jobStorage is not SqlServerStorage sqlServerStorage)
            {
                throw new InvalidOperationException("JobStorage must be an instance of SqlServerStorage.");
            }

            // Use IMonitoringApi to verify connectivity or gather storage details
            var monitoringApi = GetMonitoringApi(jobStorage);

            // Extract connection string or connection factory
            var connectionFactory = GetConnectionString(monitoringApi);
            connectionString = connectionFactory;
            _schemaName = sqlOptions.SchemaName;

            // Configure DbContext with the same connection
            var optionsBuilder = new DbContextOptionsBuilder<HangfireDbContext>();
            optionsBuilder.UseSqlServer(connectionFactory);

            // Initialize Db
            HangfireDbContext? _dbContext = serviceProvider.GetService(typeof(HangfireDbContext)) as HangfireDbContext ?? throw new Exception("HangfireDbContext is not registered in the service collection.");
            DatabaseInitializer dataInitializer = new();
            dataInitializer.InitializeDatabase(_dbContext, nomSolJobManagerOptions.GenerateSampleJob);

            var config = configuration.UseJobManager(serviceProvider); // Hook into JobManagerCore
            return config;
        }

        public static IServiceCollection AddHangfireJobManagerBusinessServices(this IServiceCollection services)
        {
            services.AddDbContext<HangfireDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<ISchedulerService, SchedulerService>();
            services.AddScoped<IHangfireJobManagerRepository, HangfireJobManagerRepository>();
            return services;
        }

        private static string GetConnectionString(MonitoringApiHelper monitoringApi)
        {
            return monitoringApi.UseConnection(connection =>
            {
                return connection.ConnectionString;
            });
        }
    }
}
