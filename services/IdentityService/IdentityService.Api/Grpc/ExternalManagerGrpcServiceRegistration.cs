using System.Reflection; 
using FinSight.Contracts; 
using Grpc.AspNetCore.Server; 
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;

namespace IdentityService.Api.Grpc; 

internal sealed record ExternalManagerGrpcService(Type ContractType, Type ImplementationType); 

internal static class ExternalManagerGrpcServiceRegistration 
{ 
    public static IReadOnlyCollection<ExternalManagerGrpcService> AddExternalManagerGrpcServices( 
        this IServiceCollection services, Assembly externalContractsAssembly, Assembly businessAssembly) 
    { 
        var externalManagerContracts = externalContractsAssembly 
            .GetTypes() 
            .Where(type => type is { IsInterface: true } && type != typeof(IExternalManager) && typeof(IExternalManager).IsAssignableFrom(type)) 
            .ToArray(); 

        var businessImplementations = businessAssembly 
            .GetTypes() 
            .Where(type => type is { IsClass: true, IsAbstract: false }) 
            .ToArray(); 

        foreach (var implementation in businessImplementations) 
        { 
            services.AddScoped(implementation); 
            foreach (var serviceInterface in implementation.GetInterfaces().Where(IsBusinessServiceInterface)) 
            { 
                services.AddScoped(serviceInterface, implementation); 
            } 
        } 

        return externalManagerContracts 
            .Select(contract => new ExternalManagerGrpcService( 
                contract, FindImplementation(contract, businessImplementations))) 
            .ToArray(); 
    } 

    public static void MapExternalManagerGrpcServices( 
        this IEndpointRouteBuilder endpoints, IEnumerable<ExternalManagerGrpcService> services) 
    { 
        var mapGrpcServiceMethod = typeof(GrpcEndpointRouteBuilderExtensions) 
            .GetMethods(BindingFlags.Public | BindingFlags.Static) 
            .First(method => method.Name == nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService) 
                             && method.IsGenericMethodDefinition 
                             && method.GetParameters().Length == 1); 

        foreach (var service in services) 
        { 
            // 🔥 FIXED: Change service.ImplementationType to service.ContractType
            // This forces protobuf-net to bind using the actual [ServiceContract] Interface!
            mapGrpcServiceMethod 
                .MakeGenericMethod(service.ContractType) 
                .Invoke(null, new object[] { endpoints }); 
        } 
    } 

    private static Type FindImplementation(Type contract, IEnumerable<Type> businessImplementations) 
    { 
        var matches = businessImplementations 
            .Where(contract.IsAssignableFrom) 
            .ToArray(); 

        return matches.Length switch { 
            1 => matches[0], 
            0 => throw new InvalidOperationException($"No Business manager implements external contract {contract.FullName}."), 
            _ => throw new InvalidOperationException($"Multiple Business managers implement external contract {contract.FullName}: {string.Join(", ", matches.Select(type => type.FullName))}") 
        }; 
    } 

    private static bool IsBusinessServiceInterface(Type type) 
    { 
        if (type == typeof(IManager) || type == typeof(IExternalManager) || type == typeof(IEngine) || type == typeof(IDatabaseRA)) 
        { 
            return false; 
        } 
        return typeof(IManager).IsAssignableFrom(type) || typeof(IEngine).IsAssignableFrom(type) || typeof(IDatabaseRA).IsAssignableFrom(type); 
    } 
}
