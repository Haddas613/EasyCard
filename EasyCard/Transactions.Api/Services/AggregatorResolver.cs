using Merchants.Business.Entities.Terminal;
using Shared.Integration.ExternalSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ClearingHouse;
using Newtonsoft.Json.Linq;
using Merchants.Business.Services;

namespace Transactions.Api.Services
{
    public class AggregatorResolver : IAggregatorResolver
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExternalSystemsService externalSystemsService;

        public AggregatorResolver(IServiceProvider serviceProvider, IExternalSystemsService externalSystemsService)
        {
            this.serviceProvider = serviceProvider;
            this.externalSystemsService = externalSystemsService;
        }

        public IAggregator GetAggregator(TerminalExternalSystem terminalExternalSystem)
        {
            var aggregator = externalSystemsService.GetExternalSystem(terminalExternalSystem.ExternalSystemID);

            // TODO: should be resolved according to integration settings
            return serviceProvider.GetService(Type.GetType(aggregator.InstanceTypeFullName)) as IAggregator;
        }

        public object GetAggregatorTerminalSettings(TerminalExternalSystem terminalExternalSystem, JObject aggregatorTerminalSettings)
        {
            var aggregator = externalSystemsService.GetExternalSystem(terminalExternalSystem.ExternalSystemID);

            return aggregatorTerminalSettings.ToObject(Type.GetType(aggregator.SettingsTypeFullName));
        }
    }
}
