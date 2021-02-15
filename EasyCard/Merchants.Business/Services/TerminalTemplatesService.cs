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
    public class TerminalTemplatesService : ServiceBase<TerminalTemplate, long>, ITerminalTemplatesService
    {
        private readonly MerchantsContext context;
        private readonly ClaimsPrincipal user;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        public TerminalTemplatesService(MerchantsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        public IQueryable<TerminalTemplate> GetQuery()
        {
            return context.TerminalTemplates.AsNoTracking();
        }

        public async Task<TerminalTemplate> GetTerminalTemplate(long terminalTemplateID)
        {
            var terminal = await GetQuery()
                    .FirstOrDefaultAsync(m => m.TerminalTemplateID == terminalTemplateID);

            if (terminal != null)
            {
                // TODO: caching
                context.Entry(terminal)
                    .Collection(b => b.Integrations)
                    .Load();
            }

            return terminal;
        }

        public IQueryable<TerminalTemplateExternalSystem> GetTerminalTemplateExternalSystems()
        {
            return context.TerminalTemplateExternalSystems;
        }

        public async Task RemoveTerminalTemplateExternalSystem(long terminalTemplateID, long externalSystemID)
        {
            if (!user.IsAdmin())
            {
                throw new SecurityException("Method acces is not allowed");
            }

            var dbEntity = await GetTerminalTemplateExternalSystems().FirstOrDefaultAsync(es => es.ExternalSystemID == externalSystemID && es.TerminalTemplateID == terminalTemplateID);

            if (dbEntity != null)
            {
                context.TerminalTemplateExternalSystems.Remove(dbEntity);
            }

            await context.SaveChangesAsync();
        }

        public async Task SaveTerminalTemplateExternalSystem(TerminalTemplateExternalSystem entity)
        {
            if (!user.IsAdmin())
            {
                throw new SecurityException("Method acces is not allowed");
            }

            var dbEntity = await GetTerminalTemplateExternalSystems().FirstOrDefaultAsync(es => es.ExternalSystemID == entity.ExternalSystemID && es.TerminalTemplateID == entity.TerminalTemplateID);

            if (dbEntity == null)
            {
                entity.Created = DateTime.UtcNow;
                context.TerminalTemplateExternalSystems.Add(entity);
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
    }
}