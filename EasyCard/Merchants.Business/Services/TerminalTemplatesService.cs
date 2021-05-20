using Merchants.Business.Data;
using Merchants.Business.Entities.Integration;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json.Linq;
using Shared.Business;
using Shared.Business.Audit;
using Shared.Business.AutoHistory;
using Shared.Business.Messages;
using Shared.Business.Security;
using Shared.Helpers.Security;
using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

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
            var templateQuery = context.TerminalTemplates
                    .Where(m => m.TerminalTemplateID == terminalTemplateID);

            var integrationsQuery = await GetTerminalTemplateExternalSystems(terminalTemplateID);

            var template = (await templateQuery.ToListAsync()).FirstOrDefault();

            if (template != null)
            {
                template.Integrations = integrationsQuery.ToList();
            }

            return template;
        }

        public async Task<IEnumerable<TerminalTemplateExternalSystem>> GetTerminalTemplateExternalSystems(long terminalTemplateID)
        {
            var externalSystems = await context.TerminalTemplateExternalSystems.Where(t => t.TerminalTemplateID == terminalTemplateID).ToListAsync();

            var shvaIntegration = externalSystems.FirstOrDefault(e => e.ExternalSystemID == ExternalSystemHelpers.ShvaExternalSystemID);

            if (shvaIntegration != null)
            {
                var settingsAsShvaTerminal = shvaIntegration.Settings.ToObject<ShvaTerminal>();

                if (settingsAsShvaTerminal != null && !string.IsNullOrWhiteSpace(settingsAsShvaTerminal.MerchantNumber)
                    && !string.IsNullOrWhiteSpace(settingsAsShvaTerminal.Password) && !string.IsNullOrWhiteSpace(settingsAsShvaTerminal.UserName))
                {
                    var shvaTerminal = await context.ShvaTerminals.FirstOrDefaultAsync(t => t.MerchantNumber == settingsAsShvaTerminal.MerchantNumber);

                    if (shvaTerminal != null)
                    {
                        shvaIntegration.Settings = JObject.FromObject(shvaTerminal);
                    }
                }
            }

            return externalSystems;
        }

        public async Task RemoveTerminalTemplateExternalSystem(long terminalTemplateID, long externalSystemID)
        {
            if (!user.IsAdmin())
            {
                throw new SecurityException("Method acces is not allowed");
            }

            var dbEntity = (await GetTerminalTemplateExternalSystems(terminalTemplateID)).FirstOrDefault(es => es.ExternalSystemID == externalSystemID);

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

            if (entity.ExternalSystemID == ExternalSystemHelpers.ShvaExternalSystemID)
            {
                var settingsAsShvaTerminal = entity.Settings.ToObject<ShvaTerminal>();

                if (settingsAsShvaTerminal != null && !string.IsNullOrWhiteSpace(settingsAsShvaTerminal.MerchantNumber)
                    && !string.IsNullOrWhiteSpace(settingsAsShvaTerminal.Password) && !string.IsNullOrWhiteSpace(settingsAsShvaTerminal.UserName))
                {
                    var shvaTerminal = await context.ShvaTerminals.FirstOrDefaultAsync(t => t.MerchantNumber == settingsAsShvaTerminal.MerchantNumber);
                    if (shvaTerminal != null)
                    {
                        shvaTerminal.UserName = settingsAsShvaTerminal.UserName;
                        shvaTerminal.Password = settingsAsShvaTerminal.Password;
                    }
                    else
                    {
                        context.ShvaTerminals.Add(new ShvaTerminal
                        {
                            MerchantNumber = settingsAsShvaTerminal.MerchantNumber,
                            Password = settingsAsShvaTerminal.Password,
                            UserName = settingsAsShvaTerminal.UserName
                        });
                    }
                }
            }

            var dbEntity = (await GetTerminalTemplateExternalSystems(entity.TerminalTemplateID)).FirstOrDefault(es => es.ExternalSystemID == entity.ExternalSystemID);

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