using Asp.Versioning;
using NomSol.Hangfire.JobManager.Core.Models;
using System.Reflection.Metadata.Ecma335;

namespace NomSol.Hangfire.JobManager.Core.Helpers
{
    public static class Helpers
    {
        public static bool IsHangfireTagsInstalled()
        {
            // Check loaded assemblies for the specific assembly or type
            var isTagsInstalled = AppDomain.CurrentDomain.GetAssemblies()
                .Any(assembly => assembly.FullName.Contains("FaceIT.Hangfire.Tags"));

            return isTagsInstalled;
        }

        public static List<CustomApiVersionModel> ReturnApiVersions()
        {
            return
            [
                new()
                {
                    Version = new ApiVersion(1),
                    VersionPath = "v1"
                }
            ];
        }
    }
}
