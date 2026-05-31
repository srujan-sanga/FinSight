using FinSight.Contracts;
using ProtoBuf;

namespace TransactionService.Internal.Contracts.DataContracts.Req;

[ProtoContract]
public sealed class GetTransactionRequest : MessageRequest
{
    [ProtoMember(10)]
    public string TransactionId { get; set; } = string.Empty;

    [ProtoMember(11)]
    public string UserId { get; set; } = string.Empty;
}