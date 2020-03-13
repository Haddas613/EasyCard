using Microsoft.EntityFrameworkCore;
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
    public class CreditCardTokenService : ServiceBase<CreditCardTokenDetails>, ICreditCardTokenService
    {
        private readonly TransactionsContext context;

        public CreditCardTokenService(TransactionsContext context) : base(context)
        {
            this.context = context;
        }

        public async override Task CreateEntity(CreditCardTokenDetails entity, IDbContextTransaction dbTransaction = null)
        {
            //TODO: audit
            await base.CreateEntity(entity, dbTransaction);
        }

        public IQueryable<CreditCardTokenDetails> GetTokens() => context.CreditCardTokenDetails;

        public async override Task UpdateEntity(CreditCardTokenDetails entity, IDbContextTransaction dbTransaction = null)
        {
            //TODO: audit
            await base.UpdateEntity(entity, dbTransaction);
        }
    }
}
