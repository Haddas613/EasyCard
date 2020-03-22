using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions;
using Transactions.Business.Entities;

namespace Transactions.Api.Extensions.Filtering
{
    public static class TransactionFilteringExtensions
    {
        public static IQueryable<PaymentTransaction> Filter(this IQueryable<PaymentTransaction> src, TransactionsFilter filter)
        {
            if (filter.TerminalID != null)
            {
                src = src.Where(t => t.TerminalID == filter.TerminalID);
            }

            return src;
        }
    }
}
