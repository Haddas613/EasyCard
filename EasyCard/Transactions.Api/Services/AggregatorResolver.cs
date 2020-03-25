using Merchants.Business.Entities.Terminal;
using Shared.Integration.ExternalSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ClearingHouse;

namespace Transactions.Api.Services
{
    public class AggregatorResolver : IAggregatorResolver
    {
        private readonly IServiceProvider serviceProvider;

        public AggregatorResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IAggregator GetAggregator(Terminal terminal)
        {
            // TODO: should be resolved according terminal settings
            return serviceProvider.GetService<ClearingHouseAggregator>();
        }
    }
}
