
using NomSol.Hangfire.JobManager.Core.Models;

namespace NomSol.Hangfire.JobManager.Core.Endpoints.v1.Services
{
    public interface IJobManagerServices
    {
        Task AddFireForgetJob(FireForgetJobRequest request);
    }
}
