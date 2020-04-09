using Merchants.Business.Entities.Terminal;
using Shared.Integration.ExternalSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ClearingHouse;
using Newtonsoft.Json.Linq;

namespace Transactions.Api.Services
{
    public class AggregatorResolver : IAggregatorResolver
    {
        private readonly IServiceProvider serviceProvider;

        public AggregatorResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IAggregator GetAggregator(TerminalExternalSystem terminalExternalSystem)
        {
            // TODO: should be resolved according to integration settings
            return serviceProvider.GetService(Type.GetType(terminalExternalSystem.ExternalSystem.InstanceTypeFullName)) as IAggregator;
        }

        public object GetAggregatorTerminalSettings(TerminalExternalSystem terminalExternalSystem, JObject aggregatorTerminalSettings)
        {
            return aggregatorTerminalSettings.ToObject(Type.GetType(terminalExternalSystem.ExternalSystem.SettingsTypeFullName));
        }
    }
}
