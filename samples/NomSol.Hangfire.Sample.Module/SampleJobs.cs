using Hangfire.Server;
using NomSol.Hangfire.JobManager.Core.Helpers;
using NomSol.Hangfire.JobManager.Core.Models;

namespace NomSol.Hangfire.Sample.Module
{
    public class SampleJobs
    {
        public async Task SampleJobCountTo100(PerformContext dashLogger, CancellationToken token)
        {
            dashLogger.CustomLogger("Sample: Can I Count To 100?", LogType.Information, true);

            //create loop to count to 100
            for (int i = 0; i <= 100; i++)
            {
                //check if the job has been cancelled
                if (token.IsCancellationRequested)
                {
                    dashLogger.CustomLogger("Sample: Job Cancelled", LogType.Warning, true);
                    return;
                }
                //log the current count
                dashLogger.CustomLogger($"Sample: {i}", LogType.Information, true);
                //simulate some work
                await Task.Delay(i);
            }

            dashLogger.CustomLogger("Sample: Yay! I Can Count!", LogType.Information, true);
        }
    }
}
