using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClearingHouse
{
    public class ClearingHouseAggregator : IAggregator
    {
        public Task<AggregatorCommitTransactionResponse> CommitTransaction(AggregatorCommitTransactionRequest transactionRequest)
        {
            throw new NotImplementedException();
        }

        public Task<AggregatorCreateTransactionResponse> CreateTransaction(AggregatorCreateTransactionRequest transactionRequest)
        {
            throw new NotImplementedException();
        }
    }
}
