using Merchants.Business.Data;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.User;
using Merchants.Business.Models.Merchant;
using Merchants.Shared.Enums;
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
            if (user.IsAdmin())
            {
                return context.MerchantHistories.AsNoTracking();
            }
            else
            {
                return context.MerchantHistories.AsNoTracking().Where(t => t.MerchantID == user.GetMerchantID());
            }
        }

        public IQueryable<Merchant> GetMerchants()
        {
            if (user.IsAdmin())
            {
                return context.Merchants.AsNoTracking();
            }
            else
            {
                return context.Merchants.AsNoTracking().Where(t => t.MerchantID == user.GetMerchantID());
            }
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

        public IQueryable<UserInfo> GetMerchantUsers(Guid merchantID)
        {
            return context.UserTerminalMappings
                .Where(d => d.MerchantID == merchantID)
                .Select(d => new UserInfo { DisplayName = d.DisplayName, Email = d.Email, UserID = d.UserID, Roles = d.Roles });
        }

        public async Task LinkUserToMerchant(UserInfo userInfo, Guid merchantID, IDbContextTransaction dbTransaction = null)
        {
            string changesStr = null;

            var existingMapping = await context.UserTerminalMappings.FirstOrDefaultAsync(m => m.UserID == userInfo.UserID && m.MerchantID == merchantID);
            if (existingMapping != null)
            {
                existingMapping.OperationDate = DateTime.UtcNow;
                existingMapping.OperationDoneBy = user.GetDoneBy();
                existingMapping.OperationDoneByID = user.GetDoneByID();
                existingMapping.Roles = userInfo.Roles;
                existingMapping.Email = userInfo.Email;
                existingMapping.DisplayName = userInfo.DisplayName;

                List<string> changes = new List<string>();

                // Must ToArray() here for excluding the AutoHistory model.
                var entries = this.context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted || e.State == EntityState.Added).ToArray();
                foreach (var entry in entries)
                {
                    changes.Add(entry.AutoHistory().Changed);
                }

                changesStr = string.Concat("[", string.Join(",", changes), "]");
            }
            else
            {
                context.UserTerminalMappings.Add(new Entities.User.UserTerminalMapping
                {
                    OperationDate = DateTime.UtcNow,
                    OperationDoneBy = user.GetDoneBy(),
                    OperationDoneByID = user.GetDoneByID(),
                    TerminalID = null,
                    MerchantID = merchantID,
                    UserID = userInfo.UserID,
                    Roles = userInfo.Roles,
                    Email = userInfo.Email,
                    DisplayName = userInfo.DisplayName
                });
            }

            var history = new MerchantHistory
            {
                OperationCode = OperationCodesEnum.UserTerminalLinkAdded,
                OperationDate = DateTime.UtcNow,
                OperationDoneBy = user?.GetDoneBy(),
                OperationDoneByID = user?.GetDoneByID(),
                MerchantID = merchantID,
                OperationDescription = changesStr,
                SourceIP = httpContextAccessor.GetIP()
            };
            context.MerchantHistories.Add(history);

            if (dbTransaction != null)
            {
                await context.SaveChangesAsync();
            }
            else
            {
                using var transaction = BeginDbTransaction();
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }

        // TODO: security
        public async Task UnLinkUserFromMerchant(Guid userID, Guid merchantID, IDbContextTransaction dbTransaction = null)
        {
            // TODO: history

            var entity = await context.UserTerminalMappings.FirstOrDefaultAsync(m => m.MerchantID == merchantID && m.UserID == userID);

            if (entity != null)
            {
                context.UserTerminalMappings.Remove(entity);

                var history = new MerchantHistory
                {
                    OperationCode = OperationCodesEnum.UserTerminalLinkRemoved,
                    OperationDate = DateTime.UtcNow,
                    OperationDoneBy = user?.GetDoneBy(),
                    OperationDoneByID = user?.GetDoneByID(),
                    MerchantID = merchantID,
                    SourceIP = httpContextAccessor.GetIP()
                };
                context.MerchantHistories.Add(history);

                if (dbTransaction != null)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    using var transaction = BeginDbTransaction();
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
        }

        public IQueryable<UserTerminalMapping> GetMerchantUsers() => context.UserTerminalMappings;

        public async Task UpdateUserStatus(UpdateUserStatusData data, IDbContextTransaction dbTransaction = null)
        {
            var entities = await context.UserTerminalMappings.Where(m => m.UserID == data.UserID).ToListAsync();

            if (entities.Count > 0)
            {
                foreach (var entity in entities)
                {
                    entity.Status = data.Status;

                    if (!string.IsNullOrEmpty(data.DisplayName))
                    {
                        entity.DisplayName = data.DisplayName;
                    }

                    if (!string.IsNullOrEmpty(data.Email))
                    {
                        entity.Email = data.Email;
                    }
                }

                if (dbTransaction != null)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    using var transaction = BeginDbTransaction();
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
        }
    }
}
