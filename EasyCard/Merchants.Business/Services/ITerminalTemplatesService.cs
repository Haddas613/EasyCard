using Merchants.Business.Entities.Terminal;
using Merchants.Business.Entities.User;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public interface ITerminalTemplatesService : IServiceBase<TerminalTemplate, long>
    {
        public IQueryable<TerminalTemplate> GetQuery();

        public Task<TerminalTemplate> GetTerminalTemplate(long terminalTemplateID);

        public Task<IEnumerable<TerminalTemplateExternalSystem>> GetTerminalTemplateExternalSystems(long terminalTemplateID);

        public Task SaveTerminalTemplateExternalSystem(TerminalTemplateExternalSystem entity);

        public Task RemoveTerminalTemplateExternalSystem(long terminalTemplateID, long externalSystemID);
    }
}
