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
            CreateMap<TerminalRequest, Terminal>()
                .ForMember(m => m.Created, o => o.MapFrom((src, tgt) => tgt.Created = DateTime.UtcNow));
            CreateMap<UpdateTerminalRequest, Terminal>();

            CreateMap<Merchants.Business.Entities.Terminal.TerminalSettings, Models.Terminal.TerminalSettings>().ReverseMap();

            CreateMap<Merchants.Business.Entities.Terminal.TerminalBillingSettings, Models.Terminal.TerminalBillingSettings>()
                .ForMember(m => m.BillingNotificationsEmails, o => o.MapFrom(
                    (src) => src.BillingNotificationsEmails.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))).ReverseMap();
            CreateMap<Terminal, TerminalResponse>();
            CreateMap<Terminal, TerminalSummary>();
            CreateMap<ExternalSystem, ExternalSystemSummary>();
            CreateMap<Feature, FeatureResponse>();

            CreateMap<TerminalExternalSystem, TerminalExternalSystemDetails>();
            CreateMap<ExternalSystemRequest, TerminalExternalSystem>();
        }
    }
}
