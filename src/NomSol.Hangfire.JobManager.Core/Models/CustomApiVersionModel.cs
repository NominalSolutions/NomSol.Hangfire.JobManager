using Asp.Versioning;

namespace NomSol.Hangfire.JobManager.Core.Models
{
    public class CustomApiVersionModel
    {
        public required ApiVersion Version { get; set; }
        public required string VersionPath { get; set; }
    }
}
