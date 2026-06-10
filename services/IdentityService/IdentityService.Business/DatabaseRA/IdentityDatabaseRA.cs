using FinSight.Contracts;
using IdentityService.Internal.Contracts.DataContracts.Entities;
using IdentityService.Internal.Contracts.ServiceContracts;
using Microsoft.EntityFrameworkCore;
namespace IdentityService.Business.DatabaseRA;

public sealed class IdentityDatabaseRA(IdentityContext context) : RaBase(), IIdentityDatabaseRA
{  
    private readonly IdentityContext _context = context;

    public async Task<DbUser?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        // Pure query. IdentityContext was already configured via your gRPC/HTTP Interceptor
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }
}
