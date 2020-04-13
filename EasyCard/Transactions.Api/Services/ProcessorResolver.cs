using Merchants.Business.Entities.Terminal;
using Shared.Integration.ExternalSystems;
using Shva;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Merchants.Business.Services;

namespace Transactions.Api.Services
{
    public class ProcessorResolver : IProcessorResolver
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExternalSystemsService externalSystemsService;

        public ProcessorResolver(IServiceProvider serviceProvider, IExternalSystemsService externalSystemsService)
        {
            this.serviceProvider = serviceProvider;
            this.externalSystemsService = externalSystemsService;
        }

        public IProcessor GetProcessor(TerminalExternalSystem terminalExternalSystem)
        {
            var processor = externalSystemsService.GetAggregator(terminalExternalSystem.ExternalSystemID);

            return serviceProvider.GetService(Type.GetType(processor.InstanceTypeFullName)) as IProcessor;
        }

        public object GetProcessorTerminalSettings(TerminalExternalSystem terminalExternalSystem, JObject processorTerminalSettings)
        {
            var processor = externalSystemsService.GetAggregator(terminalExternalSystem.ExternalSystemID);

            return processorTerminalSettings.ToObject(Type.GetType(processor.SettingsTypeFullName));
        }
    }
}
