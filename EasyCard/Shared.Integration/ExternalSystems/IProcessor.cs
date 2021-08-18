using Shared.Integration.Models;
using System;
using System.Threading.Tasks;

namespace Shared.Integration.ExternalSystems
{
    public interface IProcessor
    {
        Task<ProcessorCreateTransactionResponse> CreateTransaction(ProcessorCreateTransactionRequest paymentTransactionRequest);

        Task<ProcessorTransmitTransactionsResponse> TransmitTransactions(ProcessorTransmitTransactionsRequest transmitTransactionsRequest);

        Task ParamsUpdateTransaction(ProcessorUpdateParametersRequest updateParametersRequest);

        Task<ProcessorPreCreateTransactionResponse> PreCreateTransaction(ProcessorCreateTransactionRequest paymentTransactionRequest);
    }
}
