using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Integration.ExternalSystems
{
    public interface IAggregator
    {
        Task<AggregatorCreateTransactionResponse> CreateTransaction(AggregatorCreateTransactionRequest transactionRequest, string messageId, string
             correlationId);

        Task<AggregatorCommitTransactionResponse> CommitTransaction(AggregatorCommitTransactionRequest transactionRequest, string messageId, string
             correlationId);
    }
}
