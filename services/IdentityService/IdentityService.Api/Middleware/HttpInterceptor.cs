using Microsoft.AspNetCore.Http;
using IdentityService.Internal.Contracts.ServiceContracts;

namespace IdentityService.Api.Grpc;

public sealed class TenantHttpInterceptorMiddleware
{
    private readonly RequestDelegate _next;
    private const string TenantHeaderKey = "Internal-System-Id";

    public TenantHttpInterceptorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantAccessor tenantAccessor)
    {
        // 1. Standard Case-Insensitive Header Extraction (Handles HTTP/2 and gRPC lowercase casing)
        var tenantHeader = context.Request.Headers
            .FirstOrDefault(h => h.Key.Equals(TenantHeaderKey, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(tenantHeader.Value))
        {
            tenantAccessor.InternalSystemId = tenantHeader.Value.ToString();
        }
        else
        {
            // 2. Prototype Fallback Safety: If header is completely missing, 
            // set a default instead of throwing a messy 400 error.
            tenantAccessor.InternalSystemId = "tenant_alpha";
        }

        // 3. Clean execution hand-off. Request body is completely untouched!
        await _next(context);
    }
}
