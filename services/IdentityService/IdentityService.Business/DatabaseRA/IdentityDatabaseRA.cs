using FinSight.Contracts;
using IdentityService.Internal.Contracts.DataContracts;

namespace IdentityService.Business.DatabaseRA;

public sealed class IdentityDatabaseRA : RaBase, IIdentityDatabaseRA
{
    private static readonly IReadOnlyDictionary<string, UserDto> Users =
        new Dictionary<string, UserDto>(StringComparer.OrdinalIgnoreCase)
        {
            ["demo-user"] = new()
            {
                UserId = "demo-user",
                Email = "demo.user@finsight.local",
                DisplayName = "Demo User"
            }
        };

    public Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }
}
