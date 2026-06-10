using Grpc.Core;
using Grpc.Core.Interceptors;
namespace IdentityService.Api.Grpc;
using IdentityService.Internal.Contracts.ServiceContracts;
public class TenantGrpcInterceptor : Interceptor
{
    private readonly ITenantAccessor _tenantAccessor;

    public TenantGrpcInterceptor(ITenantAccessor tenantAccessor)
    {
        _tenantAccessor = tenantAccessor;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var metadata = context.RequestHeaders;
        var systemId = metadata.GetValue("x-internal-system-id") ?? metadata.GetValue("x-correlation-id");

        if (!string.IsNullOrEmpty(systemId))
        {
            _tenantAccessor.InternalSystemId = systemId;
        }

        return await continuation(request, context);
    }
}