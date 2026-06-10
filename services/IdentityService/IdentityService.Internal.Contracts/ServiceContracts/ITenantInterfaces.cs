namespace IdentityService.Internal.Contracts.ServiceContracts;
public interface ITenantAccessor
{
    string? InternalSystemId { get; set; }
}



// 2. Looks up raw connection metadata from your Master System/Tenant lookup DB
public interface ITenantConnectionLookup
{
    string GetConnectionString(string systemId);
}