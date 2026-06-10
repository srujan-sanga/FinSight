public sealed class TenantMapping
{
    public string SystemId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}