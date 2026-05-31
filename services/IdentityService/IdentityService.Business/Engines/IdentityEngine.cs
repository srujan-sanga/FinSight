using FinSight.Contracts;
using IdentityService.Business.DatabaseRA;
using IdentityService.Internal.Contracts.DataContracts.Req;
using IdentityService.Internal.Contracts.DataContracts.Responses;
using IdentityService.Internal.Contracts.ServiceContracts;

namespace IdentityService.Business.Engines;

public sealed class IdentityEngine : EngineBase, IIdentityEngine
{
    private readonly IIdentityDatabaseRA databaseRA;

    public IdentityEngine(IIdentityDatabaseRA databaseRA)
    {
        this.databaseRA = databaseRA;
    }

    public async Task<GetUserResponse> GetUserAsync(
        GetUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await databaseRA.GetUserAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return new GetUserResponse
            {
                Success = false,
                Message = "User was not found.",
                CorrelationId = request.CorrelationId
            };
        }

        return new GetUserResponse
        {
            Success = true,
            CorrelationId = request.CorrelationId,
            User = user
        };
    }
}
