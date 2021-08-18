using AutoMapper;
using MerchantProfileApi.Models.Terminal;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.System;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Models.Integration;
using Merchants.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MerchantProfileApi.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterTerminalMappings();
        }

        private void RegisterTerminalMappings()
        {
            CreateMap<UpdateTerminalRequest, Terminal>()
                .ForMember(d => d.CheckoutSettings, o => o.MapFrom(d => d.CheckoutSettings));

            CreateMap<TerminalSettingsUpdate, Merchants.Shared.Models.TerminalSettings>();
            CreateMap<TerminalBillingSettingsUpdate, Merchants.Shared.Models.TerminalBillingSettings>();
            CreateMap<TerminalInvoiceSettingsUpdate, Merchants.Shared.Models.TerminalInvoiceSettings>();
            CreateMap<TerminalCheckoutSettingsUpdate, Merchants.Shared.Models.TerminalCheckoutSettings>();
            CreateMap<TerminalPaymentRequestSettingsUpdate, Merchants.Shared.Models.TerminalPaymentRequestSettings>();

            CreateMap<Terminal, TerminalResponse>()
                .ForMember(d => d.Integrations, o => o.Ignore())
                .ForMember(d => d.BankDetails, o => o.MapFrom(d => d.BankDetails == null ? new TerminalBankDetails() : d.BankDetails));

            CreateMap<Terminal, TerminalSummary>();
            CreateMap<ExternalSystem, ExternalSystemSummary>();
            CreateMap<Feature, FeatureSummary>();

            CreateMap<TerminalExternalSystem, TerminalExternalSystemDetails>();
            CreateMap<ExternalSystemRequest, TerminalExternalSystem>();

            CreateMap<SystemSettings, TerminalResponse>()
                .ForMember(d => d.Settings, o => o.MapFrom(d => d.Settings))
                .ForMember(d => d.BillingSettings, o => o.MapFrom(d => d.BillingSettings))
                .ForMember(d => d.PaymentRequestSettings, o => o.MapFrom(d => d.PaymentRequestSettings))
                .ForMember(d => d.CheckoutSettings, o => o.MapFrom(d => d.CheckoutSettings))
                .ForMember(d => d.InvoiceSettings, o => o.MapFrom(d => d.InvoiceSettings))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<SystemInvoiceSettings, TerminalInvoiceSettings>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));

            CreateMap<SystemGlobalSettings, TerminalSettings>()
                .ForMember(d => d.VATRateGlobal, o => o.MapFrom(d => d.VATRate))
                .ForMember(d => d.VATRate, o => o.Ignore())
                .ForAllOtherMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));

            CreateMap<SystemPaymentRequestSettings, TerminalPaymentRequestSettings>()
              .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));
            CreateMap<SystemCheckoutSettings, TerminalCheckoutSettings>()
              .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));
            CreateMap<SystemBillingSettings, TerminalBillingSettings>()
              .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));
        }
    }
}
