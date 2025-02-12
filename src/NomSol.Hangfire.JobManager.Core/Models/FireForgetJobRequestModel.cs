
namespace NomSol.Hangfire.JobManager.Core.Models
{
    public class FireForgetJobRequest
    {
        public required long TypeId { get; set; }
        public required string JobName { get; set; }
        public string? Arguments { get; set; }
    }
}
