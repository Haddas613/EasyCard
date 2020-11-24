using AutoMapper;
using Merchants.Business.Entities.System;
using Merchants.Business.Entities.Terminal;
using Merchants.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Mapping
{
    public class TerminalProfile : Profile
    {
        public TerminalProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterTerminalMappings();
        }

        private void RegisterTerminalMappings()
        {
            CreateMap<SystemSettings, Terminal>()
                .ForMember(d => d.Settings, o => o.MapFrom(d => d.Settings))
                .ForMember(d => d.BillingSettings, o => o.MapFrom(d => d.BillingSettings))
                .ForMember(d => d.PaymentRequestSettings, o => o.MapFrom(d => d.PaymentRequestSettings))
                .ForMember(d => d.CheckoutSettings, o => o.MapFrom(d => d.CheckoutSettings))
                .ForMember(d => d.InvoiceSettings, o => o.MapFrom(d => d.InvoiceSettings))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<SystemInvoiceSettings, TerminalInvoiceSettings>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));
            CreateMap<SystemGlobalSettings, TerminalSettings>()
               .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));
            CreateMap<SystemPaymentRequestSettings, TerminalPaymentRequestSettings>()
              .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));
            CreateMap<SystemCheckoutSettings, TerminalCheckoutSettings>()
              .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));
            CreateMap<SystemBillingSettings, TerminalBillingSettings>()
              .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));
        }
    }
}
