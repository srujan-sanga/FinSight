using FinSight.Contracts;
using IdentityService.External.Contracts.ServiceContracts;
using IdentityService.Internal.Contracts.DataContracts.Requests;
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

 // 💡 NEW METHOD: Exposed via IIdentityManager for credential processing
    public async Task<LoginResponse> AuthenticateUserAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var validatedUser = await engine.VerifyUserCredentialsAsync(request.Username, request.Password, cancellationToken);

        if (validatedUser == null)
        {
            return new LoginResponse { Success = false, Message = "Invalid username or password." };
        }

        return new LoginResponse 
        { 
            Success = true, 
            Username = validatedUser.Username,
            Role = validatedUser.Role,
            DateOfBirth = validatedUser.DateOfBirth
        };
    }
}

