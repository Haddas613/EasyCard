using Merchants.Business.Entities.Terminal;
using Newtonsoft.Json.Linq;
using Shared.Integration.ExternalSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Services
{
    public interface IInvoicingResolver
    {
        IInvoicing GetInvoicing(TerminalExternalSystem terminalExternalSystem);

        object GetInvoicingTerminalSettings(TerminalExternalSystem terminalExternalSystem, JObject invocingTerminalSettings);
    }
}
