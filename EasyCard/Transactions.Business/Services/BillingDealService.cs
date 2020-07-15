using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using Shared.Business.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Data;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public class BillingDealService : ServiceBase<BillingDeal, Guid>, IBillingDealService
    {
        private readonly TransactionsContext context;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        public BillingDealService(TransactionsContext context, IHttpContextAccessorWrapper httpContextAccessor) : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async override Task CreateEntity(BillingDeal entity, IDbContextTransaction dbTransaction = null)
        {
            //entity.ApplyAuditInfo(httpContextAccessor);

            await base.CreateEntity(entity, dbTransaction);
        }

        public IQueryable<BillingDeal> GetBillingDeals() => context.BillingDeals;

        public async override Task UpdateEntity(BillingDeal entity, IDbContextTransaction dbTransaction = null)
        {
            //TODO: audit
            await base.UpdateEntity(entity, dbTransaction);
        }
    }
}
