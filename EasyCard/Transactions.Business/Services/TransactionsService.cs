using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Transactions.Business.Data;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public class TransactionsService : ITransactionsService
    {
        private readonly TransactionsContext context;

        public TransactionsService(TransactionsContext context)
        {
            this.context = context;
        }

        public IQueryable<PaymentTransaction> GetTransactions() => context.PaymentTransactions;
    }
}
