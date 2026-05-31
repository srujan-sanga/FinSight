using FinSight.Contracts;
using ProtoBuf;

namespace TransactionService.Internal.Contracts.DataContracts.Responses;

[ProtoContract]
public sealed class ListTransactionsResponse : MessageResponse
{
    [ProtoMember(10)]
    public List<TransactionDto> Transactions { get; set; } = [];

    [ProtoMember(11)]
    public int TotalCount { get; set; }
}

[ProtoContract]
public sealed class TransactionDto
{
    [ProtoMember(1)]
    public string TransactionId { get; set; } = string.Empty;

    [ProtoMember(2)]
    public string UserId { get; set; } = string.Empty;

    [ProtoMember(3)]
    public decimal Amount { get; set; }

    [ProtoMember(4)]
    public string Type { get; set; } = string.Empty;

    [ProtoMember(5)]
    public string Category { get; set; } = string.Empty;

    [ProtoMember(6)]
    public DateTime TransactionDate { get; set; }
}
