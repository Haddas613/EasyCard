using Merchants.Business.Entities.Terminal;
using Shared.Integration.ExternalSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Services
{
    public interface IProcessorResolver
    {
        IProcessor GetProcessor(TerminalExternalSystem terminalExternalSystem);
    }
}