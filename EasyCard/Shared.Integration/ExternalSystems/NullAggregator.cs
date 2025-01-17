﻿using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Integration.ExternalSystems
{
    public class NullAggregator : IAggregator
    {
        public bool AllowTransmissionCancellation()
        {
            return true;
        }

        public Task<AggregatorCancelTransactionResponse> CancelTransaction(AggregatorCancelTransactionRequest transactionRequest)
        {
            throw new NotImplementedException();
        }

        public Task<AggregatorCommitTransactionResponse> CommitTransaction(AggregatorCommitTransactionRequest transactionRequest)
        {
            throw new NotImplementedException();
        }

        public Task<AggregatorCreateTransactionResponse> CreateTransaction(AggregatorCreateTransactionRequest transactionRequest)
        {
            throw new NotImplementedException();
        }

        public Task<AggregatorTransactionResponse> GetTransaction(string aggregatorTransactionID)
        {
            throw new NotImplementedException();
        }

        public bool ShouldBeProcessedByAggregator(TransactionTypeEnum transactionType, SpecialTransactionTypeEnum specialTransactionType, JDealTypeEnum jDealType)
        {
            return false;
        }

        public Task<IEnumerable<IntegrationMessage>> GetStorageLogs(string entityID)
        {
            throw new NotImplementedException();
        }

        public string Validate(AggregatorCreateTransactionRequest transactionRequest)
        {
            throw new NotImplementedException();
        }
    }
}
