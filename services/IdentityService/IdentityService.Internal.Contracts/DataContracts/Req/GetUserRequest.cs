using FinSight.Contracts;
using ProtoBuf;

namespace IdentityService.Internal.Contracts.DataContracts.Req;

[ProtoContract]
public sealed class GetUserRequest : MessageRequest
{
    [ProtoMember(10)]
    public string UserId { get; set; } = string.Empty;
}
