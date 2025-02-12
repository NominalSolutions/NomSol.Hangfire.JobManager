using NomSol.Hangfire.JobManager.Core.Interfaces;
using NomSol.Hangfire.JobManager.Core.Models;
using NomSol.Hangfire.JobManager.Core.Models.Data.Tables;
using NomSol.Hangfire.JobManager.SqlServer.Data.Context;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NomSol.Hangfire.JobManager.SqlServer.Data.Repository
{
    internal class HangfireJobManagerRepository : IHangfireJobManagerRepository
    {

        private readonly HangfireDbContext hangfireContext;

        public HangfireJobManagerRepository(HangfireDbContext hangfireContext)
        {
            this.hangfireContext = hangfireContext;
        }


        public void MarkJobComplete(long jobId, string jobName)
        {
            hangfireContext.MarkJobComplete(jobId, jobName);
        }

        public List<Core.Models.Data.Tables.JobManager> GetAllJobs()
        {
            return hangfireContext.GetAllJobs();
        }

        public async Task AddFireForgetJob(FireForgetJobRequest request)
        {
            FireForgetJobManager fireForgetJobManager = new FireForgetJobManager
            {
                FK_JobTypeID = request.TypeId,
                JobName = request.JobName,
                Arguments = request.Arguments,
                Status = "ACTIVE",
                Created_Date = System.DateTime.Now,
                Active_Flag = false
            };

            await hangfireContext.CreateFireForgetJob(fireForgetJobManager);
        }
    }
}
