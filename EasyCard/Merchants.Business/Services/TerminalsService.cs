using Merchants.Business.Data;
using Merchants.Business.Entities.Integration;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.Terminal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using Shared.Business.Audit;
using Shared.Business.AutoHistory;
using Shared.Business.Exceptions;
using Shared.Business.Messages;
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
    public class TerminalsService : ServiceBase<Terminal>, ITerminalsService
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

        public IQueryable<Terminal> GetTerminals() => context.Terminals;

        public IQueryable<TerminalExternalSystem> GetTerminalExternalSystems() => context.TerminalExternalSystems;

        public IQueryable<ExternalSystem> GetExternalSystems() => context.ExternalSystems;

        public async Task LinkUserToTerminal(string userID, long terminalID)
        {
            // if user is already linked to terminal, throw error
            if ((await context.UserTerminalMappings.CountAsync(m => m.UserID == userID && m.TerminalID == terminalID)) > 0)
            {
                throw new EntityConflictException(ApiMessages.Conflict, nameof(Entities.User.UserTerminalMapping),
                    $"{nameof(userID)}:{userID};{nameof(terminalID)}:{terminalID}");
            }

            context.UserTerminalMappings.Add(new Entities.User.UserTerminalMapping
            {
                OperationDate = DateTime.UtcNow,
                OperationDoneBy = user.GetDoneBy(),
                OperationDoneByID = user.GetDoneByID(),
                TerminalID = terminalID,
                UserID = userID,
            });

            await context.SaveChangesAsync();
        }

        public async Task UnLinkUserFromTerminal(string userID, long terminalID)
        {
            var terminal = await context.Terminals.FirstAsync(t => t.TerminalID == terminalID);

            var entity = await context.UserTerminalMappings.FirstAsync(m => m.TerminalID == terminalID && m.UserID == userID);

            context.UserTerminalMappings.Remove(entity);

            await context.SaveChangesAsync();
        }

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

            await base.UpdateEntity(entity, dbTransaction);

            var history = new MerchantHistory
            {
                OperationCode = OperationCodesEnum.TerminalUpdated.ToString(),
                OperationDate = DateTime.UtcNow,
                OperationDoneBy = user?.GetDoneBy(),
                OperationDoneByID = user?.GetDoneByID(),
                MerchantID = entity.MerchantID,
                OperationDescription = changesStr,
                SourceIP = httpContextAccessor.GetIP()
            };
            context.MerchantHistories.Add(history);
            await context.SaveChangesAsync();
        }

        public async override Task CreateEntity(Terminal entity, IDbContextTransaction dbTransaction = null)
        {
            await base.CreateEntity(entity, dbTransaction);

            var history = new MerchantHistory
            {
                OperationCode = OperationCodesEnum.TerminalCreated.ToString(),
                OperationDate = DateTime.UtcNow,
                OperationDoneBy = user?.GetDoneBy(),
                OperationDoneByID = user?.GetDoneByID(),
                MerchantID = entity.MerchantID,
                SourceIP = httpContextAccessor.GetIP()
            };
            context.MerchantHistories.Add(history);
            await context.SaveChangesAsync();
        }

        public async Task SaveTerminalExternalSystem(TerminalExternalSystem entity)
        {
            var dbEntity = await context.TerminalExternalSystems.FirstOrDefaultAsync(es => es.ExternalSystemID == entity.ExternalSystemID && es.TerminalID == entity.TerminalID);

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
                dbEntity.ExternalProcessorReference = entity.ExternalProcessorReference;
                await context.SaveChangesAsync();
            }
        }
    }
}