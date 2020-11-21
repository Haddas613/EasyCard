﻿using AutoMapper;
using MerchantProfileApi.Models.Terminal;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.System;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Models.Integration;
using Merchants.Shared.Models;
using System;
using System.Collections.Generic;

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
            CreateMap<UpdateTerminalRequest, Terminal>();

            CreateMap<TerminalSettingsUpdate, Merchants.Shared.Models.TerminalSettings>();
            CreateMap<TerminalBillingSettingsUpdate, Merchants.Shared.Models.TerminalBillingSettings>();
            CreateMap<TerminalInvoiceSettingsUpdate, Merchants.Shared.Models.TerminalInvoiceSettings>();
            CreateMap<TerminalCheckoutSettingsUpdate, Merchants.Shared.Models.TerminalCheckoutSettings>();
            CreateMap<TerminalPaymentRequestSettingsUpdate, Merchants.Shared.Models.TerminalPaymentRequestSettings>();

            CreateMap<Terminal, TerminalResponse>();
            CreateMap<Terminal, TerminalSummary>();
            CreateMap<ExternalSystem, ExternalSystemSummary>();
            CreateMap<Feature, FeatureResponse>();

            CreateMap<TerminalExternalSystem, TerminalExternalSystemDetails>();
            CreateMap<ExternalSystemRequest, TerminalExternalSystem>();

            // Mappings for settings (override terminal settings from system settings if null)

            CreateMap<SystemSettings, TerminalResponse>()
                .ForMember(d => d.Settings, o => o.MapFrom(d => d.Settings))
                .ForMember(d => d.BillingSettings, o => o.MapFrom(d => d.BillingSettings))
                .ForMember(d => d.PaymentRequestSettings, o => o.MapFrom(d => d.PaymentRequestSettings))
                .ForMember(d => d.CheckoutSettings, o => o.MapFrom(d => d.CheckoutSettings))
                .ForMember(d => d.InvoiceSettings, o => o.MapFrom(d => d.InvoiceSettings))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<TerminalInvoiceSettings, TerminalInvoiceSettings>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));
            CreateMap<TerminalSettings, TerminalSettings>()
               .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));
            CreateMap<TerminalPaymentRequestSettings, TerminalPaymentRequestSettings>()
              .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));
            CreateMap<TerminalCheckoutSettings, TerminalCheckoutSettings>()
              .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));
            CreateMap<TerminalBillingSettings, TerminalBillingSettings>()
              .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));
        }
    }
}
