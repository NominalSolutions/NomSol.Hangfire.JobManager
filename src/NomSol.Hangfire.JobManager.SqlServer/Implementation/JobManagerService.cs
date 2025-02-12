using NomSol.Hangfire.JobManager.Core.Endpoints.v1.Services;
using NomSol.Hangfire.JobManager.Core.Interfaces;
using NomSol.Hangfire.JobManager.Core.Models;
using System.Threading.Tasks;

namespace NomSol.Hangfire.JobManager.SqlServer.Implementation
{
    public class JobManagerService : IJobManagerServices
    {
        private readonly IHangfireJobManagerRepository hangfireRepository;

        public JobManagerService(IHangfireJobManagerRepository _hangfireRepository)
        {
            this.hangfireRepository = _hangfireRepository;
        }

        public async Task AddFireForgetJob(FireForgetJobRequest request)
        {
            await hangfireRepository.AddFireForgetJob(request);
        }
    }
}