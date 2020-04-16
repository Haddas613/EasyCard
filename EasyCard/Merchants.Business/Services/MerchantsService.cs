using Merchants.Business.Data;
using Merchants.Business.Entities.Merchant;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using Shared.Business.Audit;
using Shared.Business.AutoHistory;
using Shared.Business.Security;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public class MerchantsService : ServiceBase<Merchant, Guid>, IMerchantsService
    {
        private readonly MerchantsContext context;
        private readonly ClaimsPrincipal user;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        public MerchantsService(MerchantsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            this.user = httpContextAccessor.GetUser();
        }

        public IQueryable<MerchantHistory> GetMerchantHistories()
        {
            return context.MerchantHistories;
        }

        public IQueryable<Merchant> GetMerchants()
        {
            return context.Merchants;
        }

        public async override Task UpdateEntity(Merchant entity, IDbContextTransaction dbTransaction = null)
        {
            List<string> changes = new List<string>();

            // Must ToArray() here for excluding the AutoHistory model.
            var entries = this.context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted || e.State == EntityState.Added).ToArray();
            foreach (var entry in entries)
            {
                changes.Add(entry.AutoHistory().Changed);
            }

            var changesStr = string.Concat("[", string.Join(",", changes), "]");

            if (dbTransaction != null)
            {
                await base.UpdateEntity(entity, dbTransaction);

                var history = new MerchantHistory
                {
                    OperationCode = OperationCodesEnum.MerchantUpdated,
                    MerchantID = entity.MerchantID,
                    OperationDescription = changesStr,
                };

                history.ApplyAuditInfo(httpContextAccessor);

                context.MerchantHistories.Add(history);
                await context.SaveChangesAsync();
            }
            else
            {
                using var transaction = BeginDbTransaction();
                await base.UpdateEntity(entity, transaction);

                var history = new MerchantHistory
                {
                    OperationCode = OperationCodesEnum.MerchantUpdated,
                    MerchantID = entity.MerchantID,
                    OperationDescription = changesStr,
                };

                history.ApplyAuditInfo(httpContextAccessor);

                context.MerchantHistories.Add(history);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }

        public async override Task CreateEntity(Merchant entity, IDbContextTransaction dbTransaction = null)
        {
            await base.CreateEntity(entity, dbTransaction);

            var history = new MerchantHistory
            {
                OperationCode = OperationCodesEnum.MerchantCreated,
                MerchantID = entity.MerchantID,
            };

            history.ApplyAuditInfo(httpContextAccessor);

            if (dbTransaction != null)
            {
                context.MerchantHistories.Add(history);
                await context.SaveChangesAsync();
            }
            else
            {
                using var transaction = BeginDbTransaction();
                context.MerchantHistories.Add(history);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }
    }
}
