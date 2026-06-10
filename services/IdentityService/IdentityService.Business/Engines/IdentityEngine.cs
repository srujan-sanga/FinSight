using FinSight.Contracts;
using IdentityService.Business.DatabaseRA;
using IdentityService.Internal.Contracts.DataContracts.Entities;
using IdentityService.Internal.Contracts.DataContracts.Responses;
using IdentityService.Internal.Contracts.ServiceContracts;
using BCrypt.Net;
namespace IdentityService.Business.Engines;

public sealed class IdentityEngine : EngineBase, IIdentityEngine
{
    private readonly IIdentityDatabaseRA databaseRA;

    public IdentityEngine(IIdentityDatabaseRA databaseRA)
    {
        this.databaseRA = databaseRA;
    }

     public async Task<DbUser?> VerifyUserCredentialsAsync(string username, string rawPassword, CancellationToken cancellationToken)
    {
        // 1. Call DBRA to fetch user
        var user = await databaseRA.GetUserByUsernameAsync(username, cancellationToken);
        if (user == null) return null;

        // 2. Perform the business logic calculation (Crypto evaluation)
        bool IsPasswordValid = BCrypt.Net.BCrypt.Verify(rawPassword, user.PasswordHash);
        
        return IsPasswordValid ? user : null;
    }
}
