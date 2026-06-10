using FinSight.Contracts;
using IdentityService.Internal.Contracts.DataContracts.Entities;

namespace IdentityService.Internal.Contracts.ServiceContracts;

public interface IIdentityDatabaseRA : IDatabaseRA
{
    Task<DbUser?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken);
}
