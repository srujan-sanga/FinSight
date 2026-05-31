using FinSight.Contracts;
using IdentityService.Internal.Contracts.DataContracts.Req;
using IdentityService.Internal.Contracts.DataContracts.Responses;

namespace IdentityService.Internal.Contracts.ServiceContracts;

public interface IIdentityEngine : IEngine
{
    Task<GetUserResponse> GetUserAsync(
        GetUserRequest request,
        CancellationToken cancellationToken = default);
}
