using AutoMapper;
using Merchants.Business.Entities.System;
using Merchants.Business.Entities.Terminal;
using Merchants.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Checkout;

namespace Transactions.Api.Mapping
{
    public class CheckoutProfile : Profile
    {
        public CheckoutProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterTerminalMappings();
        }

        private void RegisterTerminalMappings()
        {
            CreateMap<TerminalSettings, TerminalCheckoutCombinedSettings>();
            CreateMap<TerminalCheckoutSettings, TerminalCheckoutCombinedSettings>();
        }
    }
}
