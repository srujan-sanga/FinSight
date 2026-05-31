using FinSight.Contracts;
using ProtoBuf;

namespace TransactionService.Internal.Contracts.DataContracts.Req;

[ProtoContract]
public sealed class ListTransactionsRequest : MessageRequest
{
    [ProtoMember(10)]
    public string UserId { get; set; } = string.Empty;

    [ProtoMember(11)]
    public int PageNumber { get; set; } = 1;

    [ProtoMember(12)]
    public int PageSize { get; set; } = 10;
}
