using FinSight.Contracts;
using ProtoBuf;

namespace TransactionService.Internal.Contracts.DataContracts.Responses;

[ProtoContract]
public sealed class CreateTransactionResponse : MessageResponse
{
    [ProtoMember(10)]
    public string TransactionId { get; set; } = string.Empty;

    [ProtoMember(11)]
    public bool IsSuccess { get; set; }
}
