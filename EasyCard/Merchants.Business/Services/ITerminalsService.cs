using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Entities.User;
using Merchants.Business.Models.Audit;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using Shared.Business.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public interface ITerminalsService : IServiceBase<Terminal, Guid>
    {
        public IQueryable<Terminal> GetTerminals();

        public Task<Terminal> GetTerminal(Guid terminalID);

        public Task<IEnumerable<TerminalExternalSystem>> GetTerminalExternalSystems(Guid terminalID);

        public Task SaveTerminalExternalSystem(TerminalExternalSystem entity);

        public Task RemoveTerminalExternalSystem(Guid terminalID, long externalSystemID);

        public Task AddAuditEntry(AuditEntryData auditData);
    }
}
