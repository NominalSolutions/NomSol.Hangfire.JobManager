using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NomSol.Hangfire.JobManager.Core.Models.Data.Tables
{
    [Table("JobManager")]
    public class JobManager
    {
        [Key]
        public long PK_Job_ID { get; set; }
        [ForeignKey("HangfireJobTypes")]
        public long FK_HangfireJobTypeID { get; set; }

        [ForeignKey("JobTypes")]
        public long FK_JobTypeID { get; set; }
        public string JobName { get; set; }
        public string Arguments { get; set; }
        public string CronExpression { get; set; }
        public string Status { get; set; }
        public DateTime Created_Date { get; set; }
        public DateTime? Modified_Date { get; set; }
        public bool Active_Flag { get; set; }

        public virtual JobTypes? JobTypes { get; set; }
        public virtual HangfireJobTypes? HangfireJobTypes { get; set; }
    }

    [Table("FireForgetJobManager")]
    public class FireForgetJobManager
    {
        [Key]
        public long PK_FF_Job_ID { get; set; }

        [ForeignKey("JobTypes")]
        public long FK_JobTypeID { get; set; }
        public required string JobName { get; set; }
        public string? Arguments { get; set; }
        public required string Status { get; set; }
        public DateTime Created_Date { get; set; }
        public DateTime? Modified_Date { get; set; }
        public bool Active_Flag { get; set; }

        public virtual JobTypes? JobTypes { get; set; }
    }

    [Table("HangfireJobTypes")]
    public class HangfireJobTypes
    {
        [Key]
        public long PK_HangfireJobTypeID { get; set; }
        public required string Name { get; set; }
        public bool Active_Flag { get; set; }

    }

    [Table("JobTypes")]
    public class JobTypes
    {
        [Key]
        public long PK_JobTypeID { get; set; }
        public required string Name { get; set; }
        public bool Active_Flag { get; set; }

    }
}
