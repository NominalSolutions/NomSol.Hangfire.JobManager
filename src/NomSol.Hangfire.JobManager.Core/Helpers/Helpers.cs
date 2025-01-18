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
    }
}
