using System.Reflection;

namespace NomSol.Hangfire.JobManager.Core.Helpers
{
    public static class MethodRunner
    {
        private static readonly string DefaultAssemblyDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules");

        public static async Task Run(string folder, string assemblyName, string typeName, string methodName, object[] parameters)
        {
            try
            {
                // Append .dll if not present in the assemblyName
                if (!assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    assemblyName += ".dll";
                }

                // Construct the full assembly path
                string assemblyPath = Path.Combine(DefaultAssemblyDirectory, folder);
                assemblyPath = Path.Combine(assemblyPath, assemblyName);
                Assembly jobAssembly = Assembly.LoadFrom(assemblyPath);

                var type = (from t in jobAssembly.GetTypes()
                            where t.Name == typeName
                            select t).SingleOrDefault();

                if (type != null)
                {
                    var methodInfo = (from method in type.GetMethods()
                                      where method.Name == methodName
                                      select method).SingleOrDefault();

                    if (methodInfo != null)
                    {
                        try
                        {
                            // Check if method is async
                            bool isAsync = methodInfo.ReturnType == typeof(Task) ||
                                           (methodInfo.ReturnType.IsGenericType && methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));

                            var parametersInfo = methodInfo.GetParameters();

                            // If the parameters provided are less than the method's parameters,
                            // fill the remaining with Type.Missing to use the default values
                            if (parameters.Length < parametersInfo.Length)
                            {
                                var completeParameters = new object[parametersInfo.Length];
                                parameters.CopyTo(completeParameters, 0);

                                for (int i = parameters.Length; i < parametersInfo.Length; i++)
                                {
                                    completeParameters[i] = Type.Missing;
                                }

                                parameters = completeParameters;
                            }

                            // Check if the method is static
                            if (methodInfo.IsStatic)
                            {
                                // If method is async, await it
                                if (isAsync)
                                    await (Task)methodInfo.Invoke(null, parameters);
                                else
                                    methodInfo.Invoke(null, parameters);
                            }
                            else
                            {
                                // Create an instance of the type
                                var instance = Activator.CreateInstance(type);

                                // If method is async, await it
                                if (isAsync)
                                    await (Task)methodInfo.Invoke(instance, parameters);
                                else
                                    methodInfo.Invoke(instance, parameters);
                            }
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex.InnerException;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

    }
}
