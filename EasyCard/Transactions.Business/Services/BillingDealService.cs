using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using Shared.Business.Security;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
        private readonly ClaimsPrincipal user;

        public BillingDealService(TransactionsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        public async override Task CreateEntity(BillingDeal entity, IDbContextTransaction dbTransaction = null)
        {
            entity.ApplyAuditInfo(httpContextAccessor);

            await base.CreateEntity(entity, dbTransaction);
        }

        public IQueryable<BillingDeal> GetBillingDeals()
        {
            if (user.IsAdmin())
            {
                return context.BillingDeals.AsNoTracking();
            }
            else if (user.IsTerminal())
            {
                return context.BillingDeals.AsNoTracking().Where(t => t.TerminalID == user.GetTerminalID());
            }
            else
            {
                return context.BillingDeals.AsNoTracking().Where(t => t.MerchantID == user.GetMerchantID());
            }
        }

        public IQueryable<BillingDeal> GetBillingDealsForUpdate()
        {
            if (user.IsAdmin())
            {
                return context.BillingDeals;
            }
            else if (user.IsTerminal())
            {
                return context.BillingDeals.Where(t => t.TerminalID == user.GetTerminalID());
            }
            else
            {
                return context.BillingDeals.Where(t => t.MerchantID == user.GetMerchantID());
            }
        }

        public async override Task UpdateEntity(BillingDeal entity, IDbContextTransaction dbTransaction = null)
        {
            //TODO: audit
            await base.UpdateEntity(entity, dbTransaction);
        }
    }
}
