# NomSol.Hangfire.JobManager
NomSol Hangfire Job Manager is a robust extension for managing Hangfire jobs dynamically, designed to streamline job creation, updates, and monitoring via a database.

### Installation
1. Install the package: This library can be added to your project as a NuGet package.

```bash
dotnet add package NomSol.Hangfire.JobManager
```

### Usage

```csharp

            // add the following to generate a sample job
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
```

### Managing Jobs
A couple new tables will be added to your database to manage jobs. The tables are: 
 - JobManager
    - This table contains the job details and is used to manage recurring jobs.
 - FireForgetJobManager
    - This table contains the job details and is used to manage fire and forget jobs.

### Sample
Please refer to the sample project for a demonstration of how to use this library.