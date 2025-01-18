using Hangfire.Storage;
using System;
using System.Data.Common;
using System.Reflection;

namespace NomSol.Hangfire.JobManager.SqlServer
{
    internal class MonitoringApiHelper
    {
        private readonly IMonitoringApi _monitoringApi;
        private static Type _type;
        private static MethodInfo _useConnection;

        public MonitoringApiHelper(IMonitoringApi monitoringApi)
        {
            if (monitoringApi.GetType().Name != "SqlServerMonitoringApi")
                throw new ArgumentException("The monitor API is not implemented using SQL Server", nameof(monitoringApi));
            _monitoringApi = monitoringApi;

            // Lazy initialization of reflection metadata
            if (_type != monitoringApi.GetType())
            {
                _useConnection = null;
                _type = monitoringApi.GetType();
            }

            _useConnection ??= _type.GetTypeInfo().GetMethod("UseConnection", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new ArgumentException("The function UseConnection cannot be found.");
        }

        public T UseConnection<T>(Func<DbConnection, T> action)
        {
            var method = _useConnection.MakeGenericMethod(typeof(T));
            return (T)method.Invoke(_monitoringApi, new object[] { action });
        }
    }
}
