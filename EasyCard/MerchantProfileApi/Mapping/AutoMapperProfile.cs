using AutoMapper;
using MerchantProfileApi.Models.Terminal;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Models.Integration;
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
        }
    }
}
