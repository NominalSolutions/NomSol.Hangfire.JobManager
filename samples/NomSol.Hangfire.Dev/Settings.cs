using Asp.Versioning;

namespace NomSol.Hangfire.Dev
{
    public class CustomApiVersionModel
    {
        public required ApiVersion Version { get; set; }
        public required string VersionPath { get; set; }
    }
    public class ConnectionStrings
    {
        public string iCashDatabase { get; set; }
        public string HangfireDatabase { get; set; }
        public string HangfireSchema { get; set; }
        public string CustomSchema { get; set; }
    }

    public class Credentials
    {
        public string? UserName
        {
            get;
            set;
        }

        public string? Password
        {
            get;
            set;
        }
    }
}
