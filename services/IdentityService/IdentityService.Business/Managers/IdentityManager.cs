using FinSight.Contracts;
using IdentityService.External.Contracts.ServiceContracts;
using IdentityService.Internal.Contracts.DataContracts.Req;
using IdentityService.Internal.Contracts.DataContracts.Responses;
using IdentityService.Internal.Contracts.ServiceContracts;
using ProtoBuf.Grpc;

namespace IdentityService.Business.Managers;

public sealed class IdentityManager : ManagerBase, IIdentityManager
{
    private readonly IIdentityEngine engine;

    public IdentityManager(IIdentityEngine engine)
    {
        this.engine = engine;
    }

    public Task<GetUserResponse> GetUserAsync(
        GetUserRequest request,
        CallContext context = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Task.FromResult(Failure<GetUserResponse>(request, "UserId is required."));
        }

        return engine.GetUserAsync(request, context.CancellationToken);
    }
}
