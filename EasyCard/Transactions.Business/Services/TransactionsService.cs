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
            context.PaymentTransactions.Add(entity);

            await context.SaveChangesAsync();
        }

        public async Task CreateToken(CreditCardTokenDetails tokenDetails)
        {
            context.CreditCardTokenDetails.Add(tokenDetails);

            await context.SaveChangesAsync();
        }

        public IQueryable<PaymentTransaction> GetTransactions() => context.PaymentTransactions;

        public async override Task UpdateEntity(PaymentTransaction entity, IDbContextTransaction dbTransaction = null)
        {
            var exist = context.PaymentTransactions.Find(entity.GetID());

            context.Entry(exist).CurrentValues.SetValues(entity);

            await this.context.SaveChangesAsync();
        }
    }
}
