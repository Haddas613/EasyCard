using AutoMapper;
using Merchants.Business.Entities.Merchant;
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

            CreateMap<TerminalCheckoutSettings, TerminalCheckoutCombinedSettings>()
                .ForMember(d => d.AllowSaveCreditCard, o => o.MapFrom(d => d.AllowSaveCreditCard))
             .ForMember(d => d.DisableCancelPayment, o => o.MapFrom(d => d.DisableCancelPayment));

            CreateMap<TerminalPaymentRequestSettings, TerminalCheckoutCombinedSettings>();

            CreateMap<Merchant, TerminalCheckoutCombinedSettings>()
                .ForMember(d => d.MarketingName, o => o.MapFrom(d => string.IsNullOrWhiteSpace(d.MarketingName) ? d.BusinessName : d.MarketingName))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<Terminal, TerminalCheckoutCombinedSettings>()
                .ForMember(d => d.TerminalID, o => o.MapFrom(d => d.TerminalID))
                .ForMember(d => d.CvvRequired, o => o.MapFrom(d => d.Settings.CvvRequired))
                .ForMember(d => d.NationalIDRequired, o => o.MapFrom(d => d.Settings.NationalIDRequired))
                .ForMember(d => d.EnabledFeatures, o => o.MapFrom(d => d.EnabledFeatures))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<Merchants.Business.Entities.Billing.Consumer, ConsumerInfo>();

            CreateMap<Transactions.Business.Entities.DealDetails, ConsumerInfo>();
        }
    }
}
