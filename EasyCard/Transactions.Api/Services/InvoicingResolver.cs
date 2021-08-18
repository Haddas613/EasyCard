using Merchants.Business.Entities.Terminal;
using Merchants.Business.Services;
using Newtonsoft.Json.Linq;
using Shared.Integration.ExternalSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Services
{
    public class InvoicingResolver : IInvoicingResolver
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExternalSystemsService externalSystemsService;

        public InvoicingResolver(IServiceProvider serviceProvider, IExternalSystemsService externalSystemsService)
        {
            this.serviceProvider = serviceProvider;
            this.externalSystemsService = externalSystemsService;
        }

        public object GetInvoicingTerminalSettings(TerminalExternalSystem terminalExternalSystem, JObject invocingTerminalSettings)
        {
            var invoicing = externalSystemsService.GetExternalSystem(terminalExternalSystem.ExternalSystemID);

            return invocingTerminalSettings.ToObject(Type.GetType(invoicing.SettingsTypeFullName));
        }

        public IInvoicing GetInvoicing(TerminalExternalSystem terminalExternalSystem)
        {
            var invoicing = externalSystemsService.GetExternalSystem(terminalExternalSystem.ExternalSystemID);
            return serviceProvider.GetService(Type.GetType(invoicing.InstanceTypeFullName)) as IInvoicing;
        }
    }
}
