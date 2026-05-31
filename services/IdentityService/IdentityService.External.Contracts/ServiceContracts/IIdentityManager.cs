using FinSight.Contracts;
using IdentityService.Internal.Contracts.DataContracts.Req;
using IdentityService.Internal.Contracts.DataContracts.Responses;
using ProtoBuf.Grpc;
using System.ServiceModel;

namespace IdentityService.External.Contracts.ServiceContracts;

[ServiceContract(Name = "IdentityManager")]
public interface IIdentityManager : IExternalManager
{
    [OperationContract]
    Task<GetUserResponse> GetUserAsync(
        GetUserRequest request,
        CallContext context = default);
}
