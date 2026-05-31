using FinSight.Contracts;
using ProtoBuf.Grpc;
using System.ServiceModel;
using TransactionService.Internal.Contracts.DataContracts.Req;
using TransactionService.Internal.Contracts.DataContracts.Responses;

namespace TransactionService.Contracts.ServiceContracts;

[ServiceContract(Name = "TransactionManager")]
public interface ITransactionManager : IExternalManager
{
    [OperationContract]
    Task<CreateTransactionResponse> CreateTransactionAsync(
        CreateTransactionRequest request,
        CallContext context = default);

    [OperationContract]
    Task<GetTransactionResponse> GetTransactionAsync(
        GetTransactionRequest request,
        CallContext context = default);

    [OperationContract]
    Task<ListTransactionsResponse> ListTransactionsAsync(
        ListTransactionsRequest request,
        CallContext context = default);
}