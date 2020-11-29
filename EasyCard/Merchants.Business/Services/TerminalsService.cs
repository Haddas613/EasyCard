using Merchants.Business.Data;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using Shared.Business.Audit;
using Shared.Business.AutoHistory;
using Shared.Business.Messages;
using Shared.Business.Security;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public class TerminalsService : ServiceBase<Terminal, Guid>, ITerminalsService
    {
        private readonly MerchantsContext context;
        private readonly ClaimsPrincipal user;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        public TerminalsService(MerchantsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        public IQueryable<Terminal> GetTerminals()
        {
            if (user.IsAdmin())
            {
                return context.Terminals;
            }
            else if (user.IsTerminal())
            {
                return context.Terminals.Where(t => t.TerminalID == user.GetTerminalID());
            }
            else
            {
                return context.Terminals.Where(t => t.MerchantID == user.GetMerchantID());
            }
        }

        public async Task<Terminal> GetTerminal(Guid terminalID)
        {
            var terminal = await GetTerminals()
                    .Include(t => t.Merchant) // TODO: caching
                    .FirstOrDefaultAsync(m => m.TerminalID == terminalID);

            if (terminal != null)
            {
                // TODO: caching
                context.Entry(terminal)
                    .Collection(b => b.Integrations)
                    .Load();
            }

            return terminal;
        }

        //public IQueryable<Terminal> GetUserTerminals(Guid userID)
        //{
        //    return context.UserTerminalMappings.Where(d => d.UserID == userID).Include(d => d.Terminal).Select(d => d.Terminal);
        //}

        //public IQueryable<UserInfo> GetTerminalUsers(Guid terminalID)
        //{
        //    return context.UserTerminalMappings.Where(d => d.TerminalID == terminalID).Select(d => new UserInfo { DisplayName = d.DisplayName, Email = d.Email, UserID = d.UserID, Roles = d.Roles });
        //}

        public IQueryable<TerminalExternalSystem> GetTerminalExternalSystems()
        {
            if (user.IsAdmin())
            {
                return context.TerminalExternalSystems;
            }
            else if (user.IsTerminal())
            {
                return context.TerminalExternalSystems.Where(t => t.TerminalID == user.GetTerminalID());
            }
            else
            {
                return context.TerminalExternalSystems.Where(t => t.Terminal.MerchantID == user.GetMerchantID());
            }
        }

        //// TODO: security
        //public async Task LinkUserToTerminal(UserInfo userInfo, Terminal terminal, IDbContextTransaction dbTransaction = null)
        //{
        //    string changesStr = null;

        //    var existingMapping = await context.UserTerminalMappings.FirstOrDefaultAsync(m => m.UserID == userInfo.UserID && m.TerminalID == terminal.TerminalID);
        //    if (existingMapping != null)
        //    {
        //        existingMapping.OperationDate = DateTime.UtcNow;
        //        existingMapping.OperationDoneBy = user.GetDoneBy();
        //        existingMapping.OperationDoneByID = user.GetDoneByID();
        //        existingMapping.Roles = userInfo.Roles;
        //        existingMapping.Email = userInfo.Email;
        //        existingMapping.DisplayName = userInfo.DisplayName;

        //        List<string> changes = new List<string>();

        //        // Must ToArray() here for excluding the AutoHistory model.
        //        var entries = this.context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted || e.State == EntityState.Added).ToArray();
        //        foreach (var entry in entries)
        //        {
        //            changes.Add(entry.AutoHistory().Changed);
        //        }

        //        changesStr = string.Concat("[", string.Join(",", changes), "]");
        //    }
        //    else
        //    {
        //        context.UserTerminalMappings.Add(new Entities.User.UserTerminalMapping
        //        {
        //            OperationDate = DateTime.UtcNow,
        //            OperationDoneBy = user.GetDoneBy(),
        //            OperationDoneByID = user.GetDoneByID(),
        //            TerminalID = terminal.TerminalID,
        //            UserID = userInfo.UserID,
        //            Roles = userInfo.Roles,
        //            Email = userInfo.Email,
        //            DisplayName = userInfo.DisplayName
        //        });
        //    }

        //    var history = new MerchantHistory
        //    {
        //        OperationCode = OperationCodesEnum.UserTerminalLinkAdded,
        //        OperationDate = DateTime.UtcNow,
        //        OperationDoneBy = user?.GetDoneBy(),
        //        OperationDoneByID = user?.GetDoneByID(),
        //        MerchantID = terminal.MerchantID,
        //        OperationDescription = changesStr,
        //        SourceIP = httpContextAccessor.GetIP()
        //    };
        //    context.MerchantHistories.Add(history);

        //    if (dbTransaction != null)
        //    {
        //        await context.SaveChangesAsync();
        //    }
        //    else
        //    {
        //        using var transaction = BeginDbTransaction();
        //        await context.SaveChangesAsync();
        //        await transaction.CommitAsync();
        //    }
        //}

        //// TODO: security
        //public async Task UnLinkUserFromTerminal(Guid userID, Guid terminalID, IDbContextTransaction dbTransaction = null)
        //{
        //    // TODO: history

        //    var terminal = await context.Terminals.FirstOrDefaultAsync(t => t.TerminalID == terminalID);

        //    var entity = await context.UserTerminalMappings.FirstOrDefaultAsync(m => m.TerminalID == terminalID && m.UserID == userID);

        //    if (entity != null)
        //    {
        //        context.UserTerminalMappings.Remove(entity);

        //        var history = new MerchantHistory
        //        {
        //            OperationCode = OperationCodesEnum.UserTerminalLinkRemoved,
        //            OperationDate = DateTime.UtcNow,
        //            OperationDoneBy = user?.GetDoneBy(),
        //            OperationDoneByID = user?.GetDoneByID(),
        //            MerchantID = terminal.MerchantID,
        //            SourceIP = httpContextAccessor.GetIP()
        //        };
        //        context.MerchantHistories.Add(history);

        //        if (dbTransaction != null)
        //        {
        //            await context.SaveChangesAsync();
        //        }
        //        else
        //        {
        //            using var transaction = BeginDbTransaction();
        //            await context.SaveChangesAsync();
        //            await transaction.CommitAsync();
        //        }
        //    }
        //}

        public async override Task UpdateEntity(Terminal entity, IDbContextTransaction dbTransaction = null)
        {
            List<string> changes = new List<string>();

            // Must ToArray() here for excluding the AutoHistory model.
            var entries = this.context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted || e.State == EntityState.Added).ToArray();
            foreach (var entry in entries)
            {
                changes.Add(entry.AutoHistory().Changed);
            }

            var changesStr = string.Concat("[", string.Join(",", changes), "]");

            var history = new MerchantHistory
            {
                OperationCode = OperationCodesEnum.TerminalUpdated,
                OperationDate = DateTime.UtcNow,
                OperationDoneBy = user?.GetDoneBy(),
                OperationDoneByID = user?.GetDoneByID(),
                MerchantID = entity.MerchantID,
                OperationDescription = changesStr,
                SourceIP = httpContextAccessor.GetIP()
            };
            context.MerchantHistories.Add(history);

            if (dbTransaction != null)
            {
                await base.UpdateEntity(entity, dbTransaction);
                await context.SaveChangesAsync();
            }
            else
            {
                using var transaction = BeginDbTransaction();
                await base.UpdateEntity(entity, transaction);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }

        public async override Task CreateEntity(Terminal entity, IDbContextTransaction dbTransaction = null)
        {
            var history = new MerchantHistory
            {
                OperationCode = OperationCodesEnum.TerminalCreated,
                OperationDate = DateTime.UtcNow,
                OperationDoneBy = user?.GetDoneBy(),
                OperationDoneByID = user?.GetDoneByID(),
                MerchantID = entity.MerchantID,
                SourceIP = httpContextAccessor.GetIP()
            };

            if (dbTransaction != null)
            {
                await base.CreateEntity(entity, dbTransaction);
                context.MerchantHistories.Add(history);
                await context.SaveChangesAsync();
            }
            else
            {
                using var transaction = BeginDbTransaction();
                await base.CreateEntity(entity, dbTransaction);
                context.MerchantHistories.Add(history);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }

        public async Task SaveTerminalExternalSystem(TerminalExternalSystem entity)
        {
            if (!user.IsAdmin())
            {
                throw new SecurityException("Method acces is not allowed");
            }

            var dbEntity = await GetTerminalExternalSystems().FirstOrDefaultAsync(es => es.ExternalSystemID == entity.ExternalSystemID && es.TerminalID == entity.TerminalID);

            if (dbEntity == null)
            {
                entity.Created = DateTime.UtcNow;
                context.TerminalExternalSystems.Add(entity);
                await context.SaveChangesAsync();
            }
            else
            {
                dbEntity.UpdateTimestamp = entity.UpdateTimestamp;
                dbEntity.Settings = entity.Settings;
                dbEntity.Type = entity.Type;
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveTerminalExternalSystem(Guid terminalID, long externalSystemID)
        {
            if (!user.IsAdmin())
            {
                throw new SecurityException("Method acces is not allowed");
            }

            var dbEntity = await GetTerminalExternalSystems().FirstOrDefaultAsync(es => es.ExternalSystemID == externalSystemID && es.TerminalID == terminalID);

            if (dbEntity != null)
            {
                context.TerminalExternalSystems.Remove(dbEntity);
            }

            await context.SaveChangesAsync();
        }
    }
}