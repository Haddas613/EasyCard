using IdentityServer.Data.Entities;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Services
{
    public interface ITerminalApiKeyService : IServiceBase<TerminalApiAuthKey>
    {
        IQueryable<TerminalApiAuthKey> GetAuthKeys();

        Task Delete(long terminalApiAuthKeyID);
    }
}
