namespace NomSol.Hangfire.JobManager.Core.Models
{
    public class AssemblyJob
    {
        public string? Folder { get; set; }
        public string? AssemblyName { get; set; }
        public string? TypeName { get; set; }
        public string? MethodName { get; set; }
        public bool RetryEnabled { get; set; } = true;
        public object[]? Parameters { get; set; }
    }
}
