using AutoMapper;
using Merchants.Api.Models.Merchant;
using Merchants.Api.Models.Terminal;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.Terminal;
using System;

namespace Merchants.Api.Infrastructure.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() => RegisterMappings();

        void RegisterMappings()
        {
            RegisterMerchantMappings();
            RegisterTerminalMappings();
        }

        void RegisterTerminalMappings()
        {
            CreateMap<TerminalRequest, Terminal>()
                .ForMember(m => m.Created, o => o.MapFrom((src, tgt) => tgt.Created = DateTime.UtcNow));
            CreateMap<UpdateTerminalRequest, Terminal>();
            
            CreateMap<TerminalSettings, TerminalResponseSettings>();
            CreateMap<TerminalBillingSettings, TerminalResponseBillingSettings>();
            CreateMap<Terminal, TerminalResponse>()
                 .ForPath(m => m.BillingSettings.BillingNotificationsEmails, o => o.MapFrom((src) => 
                    src.BillingSettings.BillingNotificationsEmails.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)));
            CreateMap<Terminal, TerminalSummary>();
        }

        void RegisterMerchantMappings()
        {
            CreateMap<Merchant, MerchantSummary>();
            CreateMap<Merchant, MerchantResponse>();
        }
    }
}
