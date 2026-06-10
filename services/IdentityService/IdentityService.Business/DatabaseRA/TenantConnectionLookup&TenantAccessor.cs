using IdentityService.Internal.Contracts.ServiceContracts;using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
namespace IdentityService.Business.DatabaseRA;



public sealed class TenantConnectionLookup : ITenantConnectionLookup
{
    private readonly string _masterConnectionString;

    // Pull the master connection string directly from appsettings.json
    public TenantConnectionLookup(IConfiguration configuration)
    {
        _masterConnectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("MasterTenantConnection configuration is missing.");
    }

    public string GetConnectionString(string systemId)
    {
        const string query = "SELECT connection_string FROM tenant_mappings WHERE system_id = @SystemId AND is_active = TRUE LIMIT 1;";

        // Open a direct, lightweight raw database channel
        using var connection = new NpgsqlConnection(_masterConnectionString);
        using var command = new NpgsqlCommand(query, connection);
        
        // Always use parameters to prevent SQL injection risks
        command.Parameters.AddWithValue("@SystemId", systemId);

        try
        {
            connection.Open();
            var result = command.ExecuteScalar();

            if (result == null || result == DBNull.Value)
            {
                Console.WriteLine($"System routing aborted. Registered configuration matching tenant '{systemId}' was not found.");
            }

            return result.ToString()!;
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            // Catch raw connectivity issues securely
            throw new InvalidOperationException($"Failed to query the master tenant registry database for systemId: {systemId}", ex);
        }
    }
}


public class TenantAccessor : ITenantAccessor
{
    public string? InternalSystemId { get; set; }
}
