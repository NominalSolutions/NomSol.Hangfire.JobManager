using Hangfire.Server;
using Hangfire.Storage;
using Hangfire;
using NomSol.Hangfire.JobManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using NomSol.Hangfire.JobManager.Core.Helpers;
using static NomSol.Hangfire.JobManager.Core.Helpers.CronExpressionHelper;
using Newtonsoft.Json;
using System.Threading;
using NomSol.Hangfire.JobManager.Core.Interfaces;
using NomSol.Hangfire.JobManager.Core.Implementation;

namespace NomSol.Hangfire.JobManager.SqlServer.Implementation.Business
{
    public class SchedulerBusiness
    {
        private readonly IHangfireJobManagerRepository _hangfireRepository;
        public SchedulerBusiness(IHangfireJobManagerRepository hangfireRepository)
        {
            _hangfireRepository = hangfireRepository;
        }

        public void ScheduleJobsFromDB(PerformContext dashLogger)
        {
            dashLogger.CustomLogger("Config: Scheduling Recurring Jobs", LogType.Information, true);
            RecurringJob.AddOrUpdate("UpdateRecurringJobs", () => ScheduleJobsFromDB(null), CronExpressionHelper.GetCronInterval(Interval.Minutes, 50), new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
            });

            dashLogger.CustomLogger("Config: Getting Jobs From Database", LogType.Information, true);
            List<Core.Models.Data.Tables.JobManager> allJobs = _hangfireRepository.GetAllJobs();

            dashLogger.CustomLogger("Config: " + allJobs.Count + " Job(s) Found", LogType.Information, true);

            foreach (Core.Models.Data.Tables.JobManager job in allJobs)
            {
                if (job.Status.ToUpper() == "ACTIVE")
                {
                    dashLogger.CustomLogger("Config: Checking And Applying Updates For Job: " + job.JobName, LogType.Information, true);

                    switch (job.FK_JobTypeID)
                    {
                        case (long)ProcessType.ApiCall:
                            dashLogger.CustomLogger("Config: Scheduling API Services", LogType.Information, false);
                            ScheduleApiJobs(_hangfireRepository, (JobType)job.FK_HangfireJobTypeID, job);
                            break;
                        case (long)ProcessType.Assembly:
                            dashLogger.CustomLogger("Config: Scheduling Assembly Jobs", LogType.Information, false);
                            BuildAssemblyJob(_hangfireRepository, (JobType)job.FK_HangfireJobTypeID, job);
                            break;
                    }
                }
                else if (job.Status.ToUpper() != "ACTIVE")
                {
                    dashLogger.CustomLogger("Config: Checking if " + job.JobName + " is currently scheduled and inactivating.", LogType.Information, true);
                    RecurringJob.RemoveIfExists(job.JobName);
                }

                using var connection = JobStorage.Current.GetConnection();
                List<RecurringJobDto> recurringJobs = connection.GetRecurringJobs();

                foreach (RecurringJobDto recurringJob in recurringJobs)
                {
                    if (recurringJob.Id != "UpdateRecurringJobs")
                    {
                        if (!allJobs.Any(a => a.JobName == recurringJob.Id))
                        {
                            dashLogger.CustomLogger("Config: Checking if " + recurringJob.Id + " is currently scheduled and inactivating.", LogType.Information, true);
                            RecurringJob.RemoveIfExists(recurringJob.Id);
                        }
                    }
                }
            }
        }

        private void ScheduleApiJobs(IHangfireJobManagerRepository hangfireRepository, JobType jobType, Core.Models.Data.Tables.JobManager job)
        {
            throw new NotImplementedException();

            //HttpCallModelCustom apiRequest = JsonConvert.DeserializeObject<HttpCallModelCustom>(job.Arguments);

            //switch (jobType)
            //{
            //    case JobType.FireForget:
            //        RecurringJob.RemoveIfExists(apiRequest.JobName);
            //        BackgroundJob.Enqueue(() => TriggerRestRequest(apiRequest, null, CancellationToken.None));
            //        hangfireRepository.MarkJobComplete(job.PK_Job_ID, job.JobName);
            //        break;
            //    case JobType.Recurring:
            //        RecurringJob.AddOrUpdate(apiRequest.JobName, () => TriggerRestRequest(apiRequest, null, CancellationToken.None), apiRequest.TimeIntervals.cronSchedule, new RecurringJobOptions
            //        {
            //            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
            //        });
            //        break;
            //    case JobType.Delayed:
            //        RecurringJob.RemoveIfExists(apiRequest.JobName);
            //        BackgroundJob.Schedule(() => TriggerRestRequest(apiRequest, null, CancellationToken.None), apiRequest.TimeIntervals.timespanSchedule);
            //        break;
            //}
        }

        private void BuildAssemblyJob(IHangfireJobManagerRepository hangfireRepository, JobType jobType, Core.Models.Data.Tables.JobManager job)
        {
            JobAssemblyServices assemblyServices = new();
            AssemblyJob? assemblyJob = JsonConvert.DeserializeObject<AssemblyJob>(job.Arguments) ?? throw new Exception("Error Deserializing Assembly Job");

            switch (jobType)
            {
                case JobType.FireForget:
                    RecurringJob.RemoveIfExists(job.JobName);
                    if (assemblyJob.RetryEnabled)
                    {
                        BackgroundJob.Enqueue(() => assemblyServices.RunAssemblyJob(null, CancellationToken.None, job.JobName, assemblyJob.AssemblyName, assemblyJob.TypeName, assemblyJob.MethodName, assemblyJob.Parameters, assemblyJob.Folder));
                    }
                    else
                    {
                        BackgroundJob.Enqueue(() => assemblyServices.RunAssemblyJobWithNoRetry(null, CancellationToken.None, job.JobName, assemblyJob.AssemblyName, assemblyJob.TypeName, assemblyJob.MethodName, assemblyJob.Parameters, assemblyJob.Folder));
                    }
                    hangfireRepository.MarkJobComplete(job.PK_Job_ID, job.JobName);
                    break;
                case JobType.Recurring:
                    if (assemblyJob.RetryEnabled)
                    {
                        RecurringJob.AddOrUpdate(job.JobName, () => assemblyServices.RunAssemblyJob(null, CancellationToken.None, job.JobName, assemblyJob.AssemblyName, assemblyJob.TypeName, assemblyJob.MethodName, assemblyJob.Parameters, assemblyJob.Folder), job.CronExpression, new RecurringJobOptions
                        {
                            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
                        });
                    }
                    else
                    {
                        RecurringJob.AddOrUpdate(job.JobName, () => assemblyServices.RunAssemblyJobWithNoRetry(null, CancellationToken.None, job.JobName, assemblyJob.AssemblyName, assemblyJob.TypeName, assemblyJob.MethodName, assemblyJob.Parameters, assemblyJob.Folder), job.CronExpression, new RecurringJobOptions
                        {
                            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
                        });
                    }
                    break;
                case JobType.Delayed:
                    break;
            }
        }

    }
}
