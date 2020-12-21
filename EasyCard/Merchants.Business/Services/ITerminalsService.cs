﻿using Merchants.Business.Entities.Terminal;
using Merchants.Business.Entities.User;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public interface ITerminalsService : IServiceBase<Terminal, Guid>
    {
        public IQueryable<Terminal> GetTerminals();

        public Task<Terminal> GetTerminal(Guid terminalID);

        public IQueryable<TerminalExternalSystem> GetTerminalExternalSystems();

        public Task SaveTerminalExternalSystem(TerminalExternalSystem entity);

        public Task RemoveTerminalExternalSystem(Guid terminalID, long externalSystemID);
    }
}
