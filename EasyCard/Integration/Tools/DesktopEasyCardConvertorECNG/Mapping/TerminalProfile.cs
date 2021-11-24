using AutoMapper;
using MerchantProfileApi.Models.Terminal;
using System;
using System.Collections.Generic;
using System.Text;

namespace DesktopEasyCardConvertorECNG.Mapping
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
            CreateMap<TerminalResponse, UpdateTerminalRequest>();
                

            CreateMap<Merchants.Shared.Models.TerminalSettings, TerminalSettingsUpdate>();
            CreateMap<Merchants.Shared.Models.TerminalBillingSettings, TerminalBillingSettingsUpdate>();
            CreateMap<Merchants.Shared.Models.TerminalInvoiceSettings, TerminalInvoiceSettingsUpdate>();
            CreateMap<Merchants.Shared.Models.TerminalCheckoutSettings, TerminalCheckoutSettingsUpdate>();
            CreateMap<Merchants.Shared.Models.TerminalPaymentRequestSettings, TerminalPaymentRequestSettingsUpdate>();
        }
    }
}
