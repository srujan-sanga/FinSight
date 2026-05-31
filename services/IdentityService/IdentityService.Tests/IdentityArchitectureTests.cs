using FinSight.Contracts;
using IdentityService.Business.DatabaseRA;
using IdentityService.Business.Engines;
using IdentityService.Business.Managers;
using IdentityService.External.Contracts.ServiceContracts;
using IdentityService.Internal.Contracts.DataContracts.Req;
using IdentityService.Internal.Contracts.DataContracts.Responses;
using IdentityService.Internal.Contracts.ServiceContracts;
using System.Reflection;
using System.ServiceModel;
using System.Xml.Linq;

namespace IdentityService.Tests;

public sealed class IdentityArchitectureTests
{
    [Fact]
    public void ManagerImplementsExternalContractAndBase()
    {
        var manager = new IdentityManager(new StubEngine());

        Assert.IsAssignableFrom<IIdentityManager>(manager);
        Assert.IsAssignableFrom<ManagerBase>(manager);
    }

    [Fact]
    public void EngineImplementsInternalContractAndBase()
    {
        var engine = new IdentityEngine(new IdentityDatabaseRA());

        Assert.IsAssignableFrom<IIdentityEngine>(engine);
        Assert.IsAssignableFrom<EngineBase>(engine);
    }

    [Fact]
    public void DatabaseRAImplementsRaBase()
    {
        var databaseRA = new IdentityDatabaseRA();

        Assert.IsAssignableFrom<IIdentityDatabaseRA>(databaseRA);
        Assert.IsAssignableFrom<RaBase>(databaseRA);
    }

    [Fact]
    public async Task ManagerDelegatesValidRequestToEngine()
    {
        var engine = new StubEngine();
        var manager = new IdentityManager(engine);
        var request = new GetUserRequest
        {
            UserId = "demo-user",
            CorrelationId = "correlation-1"
        };

        var response = await manager.GetUserAsync(request);

        Assert.True(engine.WasCalled);
        Assert.True(response.Success);
        Assert.Equal("correlation-1", response.CorrelationId);
    }

    [Fact]
    public async Task ManagerRejectsInvalidRequestBeforeEngine()
    {
        var engine = new StubEngine();
        var manager = new IdentityManager(engine);

        var response = await manager.GetUserAsync(new GetUserRequest());

        Assert.False(engine.WasCalled);
        Assert.False(response.Success);
        Assert.Equal("UserId is required.", response.Message);
    }

    [Fact]
    public async Task EngineCallsDatabaseRA()
    {
        var engine = new IdentityEngine(new IdentityDatabaseRA());
        var request = new GetUserRequest
        {
            UserId = "demo-user",
            CorrelationId = "correlation-2"
        };

        var response = await engine.GetUserAsync(request);

        Assert.True(response.Success);
        Assert.Equal("demo.user@finsight.local", response.User?.Email);
        Assert.Equal("correlation-2", response.CorrelationId);
    }

    [Fact]
    public void ApiReferencesExternalContractsOnly()
    {
        var repoRoot = FindRepoRoot();
        var apiProjectPath = Path.Combine(
            repoRoot,
            "services",
            "IdentityService",
            "IdentityService.Api",
            "IdentityService.Api.csproj");
        var project = XDocument.Load(apiProjectPath);
        var references = project
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value ?? string.Empty)
            .ToArray();

        Assert.Contains(
            references,
            reference => reference.EndsWith(
                "IdentityService.External.Contracts.csproj",
                StringComparison.Ordinal));
        Assert.DoesNotContain(
            references,
            reference => reference.Contains(
                "IdentityService.Business",
                StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            references,
            reference => reference.Contains(
                "IdentityService.Internal.Contracts",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ExternalManagerIsGrpcServiceContract()
    {
        var serviceContract = typeof(IIdentityManager)
            .GetCustomAttribute<ServiceContractAttribute>();
        var operationContract = typeof(IIdentityManager)
            .GetMethod(nameof(IIdentityManager.GetUserAsync))
            ?.GetCustomAttribute<OperationContractAttribute>();

        Assert.NotNull(serviceContract);
        Assert.Equal("IdentityManager", serviceContract.Name);
        Assert.NotNull(operationContract);
    }

    private sealed class StubEngine : EngineBase, IIdentityEngine
    {
        public bool WasCalled { get; private set; }

        public Task<GetUserResponse> GetUserAsync(
            GetUserRequest request,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;

            return Task.FromResult(new GetUserResponse
            {
                Success = true,
                CorrelationId = request.CorrelationId
            });
        }
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "README.md"))
                && Directory.Exists(Path.Combine(directory.FullName, "services")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find the repository root.");
    }
}
