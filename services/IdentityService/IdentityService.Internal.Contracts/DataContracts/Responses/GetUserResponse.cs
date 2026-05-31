using FinSight.Contracts;
using ProtoBuf;

namespace IdentityService.Internal.Contracts.DataContracts.Responses;

[ProtoContract]
public sealed class GetUserResponse : MessageResponse
{
    [ProtoMember(10)]
    public UserDto? User { get; set; }
}
