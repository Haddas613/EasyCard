using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Data;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public class TransactionsService : ServiceBase<PaymentTransaction>, ITransactionsService
    {
        private readonly TransactionsContext context;

        public TransactionsService(TransactionsContext context) : base(context)
        {
            this.context = context;
        }

        public async override Task CreateEntity(PaymentTransaction entity, IDbContextTransaction dbTransaction = null)
        {
            //TODO: audit
            await base.CreateEntity(entity, dbTransaction);
        }


        public IQueryable<CreditCardTokenDetails> GetTokens() => context.CreditCardTokenDetails;

        public IQueryable<PaymentTransaction> GetTransactions() => context.PaymentTransactions;

        public async override Task UpdateEntity(PaymentTransaction entity, IDbContextTransaction dbTransaction = null)
        {
            //TODO: audit
            await base.UpdateEntity(entity, dbTransaction);
        }
    }
}
