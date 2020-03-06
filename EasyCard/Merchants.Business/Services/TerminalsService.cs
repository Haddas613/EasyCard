using Merchants.Business.Data;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.Terminal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using Shared.Business.Audit;
using Shared.Business.AutoHistory;
using Shared.Business.Security;
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

        public IQueryable<Terminal> GetTerminals()
        {
            return context.Terminals;
        }

        public async Task LinkUserToTerminal(string userID, long terminalID)
        {
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
    }
}
