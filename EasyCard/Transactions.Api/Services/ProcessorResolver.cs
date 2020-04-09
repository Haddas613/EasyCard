using Merchants.Business.Entities.Terminal;
using Shared.Integration.ExternalSystems;
using Shva;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Transactions.Api.Services
{
    public class ProcessorResolver : IProcessorResolver
    {
        private readonly IServiceProvider serviceProvider;

        public ProcessorResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IProcessor GetProcessor(TerminalExternalSystem terminalExternalSystem)
        {
            return serviceProvider.GetService(Type.GetType(terminalExternalSystem.ExternalSystem.InstanceTypeFullName)) as IProcessor;
        }

        public object GetProcessorTerminalSettings(TerminalExternalSystem terminalExternalSystem, JObject processorTerminalSettings)
        {
            return processorTerminalSettings.ToObject(Type.GetType(terminalExternalSystem.ExternalSystem.SettingsTypeFullName));
        }
    }
}
