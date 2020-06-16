﻿using Merchants.Business.Entities.Terminal;
using Shared.Business;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public interface ITerminalsService : IServiceBase<Terminal, Guid>
    {
        public IQueryable<Terminal> GetTerminals();

        public IQueryable<TerminalExternalSystem> GetTerminalExternalSystems();

        public Task LinkUserToTerminal(Guid userID, Guid terminalID);

        public Task UnLinkUserFromTerminal(Guid userID, Guid terminalID);

        public Task SaveTerminalExternalSystem(TerminalExternalSystem entity);

        public Task RemoveTerminalExternalSystem(Guid terminalID, long externalSystemID);
    }
}
