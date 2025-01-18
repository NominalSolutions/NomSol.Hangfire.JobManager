using Hangfire.Server;
using NomSol.Hangfire.JobManager.Core.Models;
using Serilog;
using Hangfire.Console;

namespace NomSol.Hangfire.JobManager.Core.Helpers
{
    public static class CustomLoggerHelper
    {
        public static void CustomLogger(this PerformContext dashLogger, string logEvent, LogType type, bool toDashboard)
        {
            switch (type)
            {
                case LogType.Information:
                    Log.Information(logEvent);
                    break;
                case LogType.Warning:
                    Log.Warning(logEvent);
                    break;
                case LogType.Error:
                    Log.Error(logEvent);
                    break;
                case LogType.Fatal:
                    Log.Fatal(logEvent);
                    break;
            }

            if (toDashboard)
            {
                dashLogger.WriteLine(logEvent);
            }
        }
    }
}
