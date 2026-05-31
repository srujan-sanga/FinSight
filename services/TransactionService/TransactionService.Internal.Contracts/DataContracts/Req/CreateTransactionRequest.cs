using FinSight.Contracts;
using ProtoBuf;

namespace TransactionService.Internal.Contracts.DataContracts.Req;

[ProtoContract]
public sealed class CreateTransactionRequest : MessageRequest
{
    [ProtoMember(10)]
    public string UserId { get; set; } = string.Empty;

    [ProtoMember(11)]
    public decimal Amount { get; set; }

    [ProtoMember(12)]
    public string Type { get; set; } = string.Empty; // Expense, Income, Transfer

    [ProtoMember(13)]
    public string Category { get; set; } = string.Empty;

    [ProtoMember(14)]
    public string Description { get; set; } = string.Empty;

    [ProtoMember(15)]
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    [ProtoMember(16)]
    public bool IsRecurring { get; set; }
}