using ProtoBuf;
using FinSight.Contracts;
namespace IdentityService.Internal.Contracts.DataContracts.Requests;

[ProtoContract]
public sealed class LoginRequest : MessageRequest
{
    [ProtoMember(1)]
    public string Username { get; set; } = string.Empty;

    [ProtoMember(2)]
    public string Password { get; set; } = string.Empty;
}