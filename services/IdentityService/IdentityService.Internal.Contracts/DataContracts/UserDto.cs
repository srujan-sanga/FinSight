using ProtoBuf;

namespace IdentityService.Internal.Contracts.DataContracts;

[ProtoContract]
public sealed class UserDto
{
    [ProtoMember(1)]
    public string UserId { get; set; } = string.Empty;

    [ProtoMember(2)]
    public string Email { get; set; } = string.Empty;

    [ProtoMember(3)]
    public string DisplayName { get; set; } = string.Empty;
}
