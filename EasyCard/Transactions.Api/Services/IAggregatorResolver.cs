using Merchants.Business.Entities.Terminal;
using Newtonsoft.Json.Linq;
using Shared.Integration.ExternalSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Services
{
    public interface IAggregatorResolver
    {
        IAggregator GetAggregator(TerminalExternalSystem terminalExternalSystem);

        object GetAggregatorTerminalSettings(TerminalExternalSystem terminalExternalSystem, JObject aggregatorTerminalSettings);
    }
}
