using NomSol.Hangfire.JobManager.Core.Interfaces;
using NomSol.Hangfire.JobManager.SqlServer.Data.Context;
using System.Collections.Generic;

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
    }
}
