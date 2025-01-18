using Hangfire.SqlServer;

namespace NomSol.Hangfire.JobManager.SqlServer.Services
{
    public class SqlJobManagerServiceStorage
    {
        private readonly SqlServerStorageOptions _options;
        public SqlJobManagerServiceStorage(SqlServerStorageOptions options = null)
        {
            _options = options ?? new SqlServerStorageOptions();
        }
    }
}
