using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using NomSol.Hangfire.JobManager.Core.Models.Data.Tables;
using System.Threading.Tasks;

namespace NomSol.Hangfire.JobManager.SqlServer.Data.Context
{
    public class HangfireDbContext : DbContext
    {
        public HangfireDbContext(DbContextOptions<HangfireDbContext> options) : base(options) { }

        public virtual DbSet<Core.Models.Data.Tables.JobManager> JobManager { get; set; }
        public virtual DbSet<FireForgetJobManager> FireForgetJobManager { get; set; }
        public virtual DbSet<HangfireJobTypes> HangfireJobTypes { get; set; }
        public virtual DbSet<JobTypes> Types { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Core.Models.Data.Tables.JobManager>()
                .ToTable("JobManager", schema: ServiceCollectionExtension._schemaName);
            modelBuilder.Entity<FireForgetJobManager>()
                .ToTable("FireForgetJobManager", schema: ServiceCollectionExtension._schemaName);
            modelBuilder.Entity<JobTypes>()
                .ToTable("JobTypes", schema: ServiceCollectionExtension._schemaName);
            modelBuilder.Entity<HangfireJobTypes>()
                .ToTable("HangfireJobTypes", schema: ServiceCollectionExtension._schemaName);
        }

        internal List<Core.Models.Data.Tables.JobManager> GetAllJobs()
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            List<Core.Models.Data.Tables.JobManager> jm = [.. JobManager.Where(a => !a.Active_Flag)];
            List<FireForgetJobManager> ffj = [.. FireForgetJobManager.Where(a => !a.Active_Flag && a.Status == "ACTIVE")];

            foreach (FireForgetJobManager ff in ffj)
            {
                jm.Add(new Core.Models.Data.Tables.JobManager
                {
                    PK_Job_ID = ff.PK_FF_Job_ID,
                    FK_HangfireJobTypeID = 1,
                    FK_JobTypeID = ff.FK_JobTypeID,
                    JobName = ff.JobName,
                    Arguments = ff.Arguments,
                    Status = ff.Status,
                    Created_Date = ff.Created_Date,
                    Modified_Date = ff.Modified_Date,
                    Active_Flag = ff.Active_Flag
                });
            }

            return jm;
        }

        internal void MarkJobComplete(long jobId, string jobName)
        {
            try
            {
                Core.Models.Data.Tables.JobManager job = JobManager.AsTracking().FirstOrDefault(a => a.PK_Job_ID == jobId && a.JobName == jobName);

                job.Modified_Date = DateTime.Now;
                job.Status = "COMPLETE";
                SaveChanges();
            }
            catch
            {
                FireForgetJobManager job = FireForgetJobManager.AsTracking().FirstOrDefault(a => a.PK_FF_Job_ID == jobId && a.JobName == jobName);

                job.Modified_Date = DateTime.Now;
                job.Status = "COMPLETE";
                SaveChanges();
            }
        }

        //public async Task<removeScheduledJobResponse> removeScheduledJobByName(string jobName)
        //{
        //    Tables.JobManager job = JobManager.AsTracking().FirstOrDefault(a => a.JobName == jobName);

        //    job.Modified_Date = DateTime.Now;
        //    job.Status = "INACTIVE";
        //    await SaveChangesAsync();

        //    return new removeScheduledJobResponse
        //    {
        //        JobName = job.JobName,
        //        ScheduledJobId = job.PK_Job_ID,
        //        Status = job.Status,
        //        CreatedDate = job.Created_Date,
        //        ModifiedDate = job.Modified_Date
        //    };
        //}

        //public async Task<enableScheduledJobResponse> enableScheduledJobByName(string jobName)
        //{
        //    Tables.JobManager job = JobManager.AsTracking().FirstOrDefault(a => a.JobName == jobName);

        //    job.Modified_Date = DateTime.Now;
        //    job.Status = "ACTIVE";

        //    await SaveChangesAsync();

        //    return new enableScheduledJobResponse
        //    {
        //        JobName = job.JobName,
        //        ScheduledJobId = job.PK_Job_ID,
        //        Status = job.Status,
        //        CreatedDate = job.Created_Date,
        //        ModifiedDate = job.Modified_Date
        //    };
        //}

        internal void CreateApiJob(Core.Models.Data.Tables.JobManager job)
        {
            JobManager.Add(job);
            SaveChanges();
        }

        internal async Task CreateFireForgetJob(FireForgetJobManager job)
        {
            FireForgetJobManager.Add(job);
            await SaveChangesAsync();
        }
    }

    //public static class LookUp
    //{
    //    public static async Task<getScheduledJobResponse> getScheduledJobByName(this HangfireDBContext context, string jobName)
    //    {
    //        Tables.JobManager job = await context.JobManager.Include(d => d.JobTypes).Include(b => b.Types).FirstOrDefaultAsync(a => a.JobName == jobName);

    //        return new getScheduledJobResponse
    //        {
    //            JobName = job.JobName,
    //            ScheduledJobId = job.PK_Job_ID,
    //            Status = job.Status,
    //            CreatedDate = job.Created_Date,
    //            ModifiedDate = job.Modified_Date,
    //            ScheduleType = job.JobTypes.TypeName,
    //            JobType = job.Types.TypeName,
    //            Arguments = job.Arguments,
    //            CronSchedule = job.CronExpression
    //        };
    //    }
    //}

}
