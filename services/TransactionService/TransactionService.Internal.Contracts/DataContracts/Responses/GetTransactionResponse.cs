using FinSight.Contracts;
using ProtoBuf;

namespace TransactionService.Internal.Contracts.DataContracts.Responses;

[ProtoContract]
public sealed class GetTransactionResponse : MessageResponse
{
    [ProtoMember(10)]
    public string TransactionId { get; set; } = string.Empty;

    [ProtoMember(11)]
    public string UserId { get; set; } = string.Empty;

    [ProtoMember(12)]
    public decimal Amount { get; set; }

    [ProtoMember(13)]
    public string Type { get; set; } = string.Empty;

    [ProtoMember(14)]
    public string Category { get; set; } = string.Empty;

    [ProtoMember(15)]
    public DateTime TransactionDate { get; set; }
}
