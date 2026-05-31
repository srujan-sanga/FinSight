using System.Reflection;
using System.Runtime.Loader;

namespace IdentityService.Api.Grpc;

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

        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var projectDirectory = Path.Combine(directory.FullName, assemblyName);

            if (Directory.Exists(projectDirectory))
            {
                foreach (var candidate in Directory
                    .EnumerateFiles(projectDirectory, $"{assemblyName}.dll", SearchOption.AllDirectories)
                    .OrderByDescending(IsBuildOutput)
                    .ThenBy(path => path, StringComparer.OrdinalIgnoreCase))
                {
                    yield return candidate;
                }
            }

            directory = directory.Parent;
        }
    }

    private static bool IsBuildOutput(string path)
    {
        var binSegment = $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}";
        return path.Contains(binSegment, StringComparison.OrdinalIgnoreCase);
    }
}
