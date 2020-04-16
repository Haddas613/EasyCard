using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Validation.Options;

namespace Transactions.Api.Validation
{
    public class TransactionsFilterValidator
    {
        public static TransactionsFilter ValidateFilter(TransactionsFilter filter, TransactionFilterValidationOptions opts)
        {
            if (filter.Take > opts.MaximumPageSize)
            {
                filter.Take = opts.MaximumPageSize;
            }

            return filter;
        }
    }
}
