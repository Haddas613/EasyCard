using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public Task CreateEntity(PaymentTransaction entity, IDbContextTransaction dbTransaction = null)
        {
            throw new NotImplementedException();
        }

        public IQueryable<PaymentTransaction> GetTransactions() => context.PaymentTransactions;

        public Task UpdateEntity(PaymentTransaction entity, IDbContextTransaction dbTransaction = null)
        {
            throw new NotImplementedException();
        }
    }
}
