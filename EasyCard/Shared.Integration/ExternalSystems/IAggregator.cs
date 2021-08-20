using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Integration.ExternalSystems
{
    public interface IAggregator
    {
        Task<AggregatorCreateTransactionResponse> CreateTransaction(AggregatorCreateTransactionRequest transactionRequest);

        Task<AggregatorCommitTransactionResponse> CommitTransaction(AggregatorCommitTransactionRequest transactionRequest);

        Task<AggregatorCancelTransactionResponse> CancelTransaction(AggregatorCancelTransactionRequest transactionRequest);

        bool ShouldBeProcessedByAggregator(TransactionTypeEnum transactionType, SpecialTransactionTypeEnum specialTransactionType, JDealTypeEnum jDealType);

        /// <summary>
        ///  Validate data if it's compatible with aggregator requirements
        /// </summary>
        /// <param name="transactionRequest"></param>
        /// <returns>Error message when there is an error, null otherwise</returns>
        string Validate(AggregatorCreateTransactionRequest transactionRequest);

        bool AllowTransmissionCancellation();

        Task<AggregatorTransactionResponse> GetTransaction(string aggregatorTransactionID);
    }
}
