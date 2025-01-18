using Hangfire.Server;

namespace NomSol.Hangfire.JobManager.Core.Interfaces
{
    public interface IJobAssemblyServices
    {
        Task RunAssemblyJob(PerformContext dashLogger, CancellationToken token, string jobName, string assembly, string type, string method, object[] parameters, string folder);
        Task RunAssemblyJobWithNoRetry(PerformContext dashLogger, CancellationToken token, string jobName, string assembly, string type, string method, object[] parameters, string folder);
    }
}
