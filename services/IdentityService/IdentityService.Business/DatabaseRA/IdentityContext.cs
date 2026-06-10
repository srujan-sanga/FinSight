using IdentityService.Internal.Contracts.ServiceContracts;
using IdentityService.Internal.Contracts.DataContracts.Entities ;
using Microsoft.EntityFrameworkCore;
namespace IdentityService.Business.DatabaseRA;

public class IdentityContext : DbContext
{
    private readonly ITenantAccessor _tenantAccessor;
    private readonly ITenantConnectionLookup _connectionLookup;

    public IdentityContext(
        ITenantAccessor tenantAccessor,
        ITenantConnectionLookup connectionLookup) : base()
    {
        _tenantAccessor = tenantAccessor;
        _connectionLookup = connectionLookup;
    }

  public DbSet<DbUser> Users => Set<DbUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 💡 Fluent API Database-First Mapping
        modelBuilder.Entity<DbUser>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasMaxLength(36);
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(150).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth").HasColumnType("date").IsRequired();

            // Setup index for high speed login lookups
            entity.HasIndex(e => e.Username).IsUnique();
        });
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var systemId = _tenantAccessor.InternalSystemId;
            
            if (string.IsNullOrEmpty(systemId))
            {
                throw new InvalidOperationException("Tenant Context missing. Request Interceptor extraction aborted.");
            }

            // 💡 Fetch the connection string based on the tenant ID at runtime
            string connectionString = _connectionLookup.GetConnectionString(systemId);
            
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
    
    // Add your DB Sets here...
}