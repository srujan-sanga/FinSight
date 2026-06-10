using FinSight.Contracts;
using IdentityService.Internal.Contracts.DataContracts.Requests;
using IdentityService.Internal.Contracts.DataContracts.Responses;
using ProtoBuf.Grpc;
using System.ServiceModel;

namespace IdentityService.External.Contracts.ServiceContracts;

[ServiceContract(Name = "IdentityManager")]
public interface IIdentityManager : IExternalManager
{
    [OperationContract]
    public  Task<LoginResponse> AuthenticateUserAsync(LoginRequest request, CancellationToken cancellationToken);

}
