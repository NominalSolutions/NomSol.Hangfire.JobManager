using Hangfire.Server;
using Hangfire;
using NomSol.Hangfire.JobManager.Core.Models;
using System.ComponentModel;
using NomSol.Hangfire.JobManager.Core.Interfaces;
using NomSol.Hangfire.JobManager.Core.Helpers;
using NomSol.Hangfire.JobGuard.Attributes;

namespace NomSol.Hangfire.JobManager.Core.Implementation
{

    public class JobAssemblyServices : IJobAssemblyServices
    {
        public JobAssemblyServices()
        {
        }

        [PreventDuplicateJob]
        [AutomaticRetry(Attempts = 3)]
        [DisableConcurrentExecution(timeoutInSeconds: 60)]
        [DisplayName("{3}.{5}")]
        public async Task RunAssemblyJob(PerformContext dashLogger, CancellationToken token, string jobName, string assembly, string type, string method, object[] parameters, string folder)
        {
            dashLogger.CustomLogger("MethodRunner: Executing: " + assembly + "." + type + "." + method, LogType.Information, true);

            if (Helpers.Helpers.IsHangfireTagsInstalled())
            {
                //todo: add tags
                //dashLogger.AddTags(jobName);
            }

            object[] param = new object[] { dashLogger, token };
            if (parameters != null)
            {
                param = param.Concat(parameters).ToArray();
            }

            //Load Assembly and run method
            await MethodRunner.Run(folder, assembly, type, method, param);

            dashLogger.CustomLogger("MethodRunner: Execution Complete", LogType.Information, true);
        }

        [PreventDuplicateJob]
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(timeoutInSeconds: 60)]
        [DisplayName("{3}.{5}")]
        public async Task RunAssemblyJobWithNoRetry(PerformContext dashLogger, CancellationToken token, string jobName, string assembly, string type, string method, object[] parameters, string folder)
        {
            dashLogger.CustomLogger("MethodRunner: Executing: " + assembly + "." + type + "." + method, LogType.Information, true);


            if (Helpers.Helpers.IsHangfireTagsInstalled())
            {
                //todo: add tags
                //dashLogger.AddTags(jobName);
            }

            object[] param = new object[] { dashLogger, token };
            if (parameters != null)
            {
                param = param.Concat(parameters).ToArray();
            }

            //Load Assembly and run method
            await MethodRunner.Run(folder, assembly, type, method, param);

            dashLogger.CustomLogger("MethodRunner: Execution Complete", LogType.Information, true);
        }
    }
}
