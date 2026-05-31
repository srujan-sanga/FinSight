using System.Reflection;
using System.Runtime.Loader;

namespace TransactionService.Api.Grpc;

internal static class BusinessAssemblyLoader
{
    public static Assembly Load(string assemblyName)
    {
        var loadedAssembly = AppDomain.CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(assembly => assembly.GetName().Name == assemblyName);

        if (loadedAssembly is not null)
        {
            return loadedAssembly;
        }

        var assemblyPath = EnumerateAssemblyCandidates(assemblyName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(File.Exists);

        if (assemblyPath is null)
        {
            throw new InvalidOperationException(
                $"Could not find {assemblyName}.dll. Build the Business project or set FINSIGHT_BUSINESS_ASSEMBLY_PATH.");
        }

        var assemblyDirectory = Path.GetDirectoryName(assemblyPath)!;
        AssemblyLoadContext.Default.Resolving += (_, name) =>
        {
            var dependencyPath = Path.Combine(assemblyDirectory, $"{name.Name}.dll");
            return File.Exists(dependencyPath)
                ? AssemblyLoadContext.Default.LoadFromAssemblyPath(dependencyPath)
                : null;
        };

        return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
    }

    private static IEnumerable<string> EnumerateAssemblyCandidates(string assemblyName)
    {
        var configuredPath = Environment.GetEnvironmentVariable("FINSIGHT_BUSINESS_ASSEMBLY_PATH");

        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            yield return configuredPath;
        }

        yield return Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.dll");
    }
}
