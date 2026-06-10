using ProtoBuf;
using FinSight.Contracts;
namespace IdentityService.Internal.Contracts.DataContracts.Responses;

[ProtoContract]
public sealed class LoginResponse:MessageResponse
{
    [ProtoMember(1)]
    public string Username { get; set; } = string.Empty;

    [ProtoMember(4)]
    public string Role { get; set; } = "User";

    [ProtoMember(5)]
    public DateTime DateOfBirth { get; set; }
}
