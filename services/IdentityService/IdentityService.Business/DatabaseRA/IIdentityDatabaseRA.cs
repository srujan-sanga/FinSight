using FinSight.Contracts;
using IdentityService.Internal.Contracts.DataContracts;

namespace IdentityService.Business.DatabaseRA;

public interface IIdentityDatabaseRA : IDatabaseRA
{
    Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
}
