using AutoMapper;
using IdentityServerClient;
using Merchants.Api.Models;
using Merchants.Api.Models.Merchant;
using Merchants.Api.Models.Terminal;
using Merchants.Api.Models.User;
using Merchants.Business.Entities.Integration;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.Terminal;
using System;
using System.Collections.Generic;

namespace Merchants.Api.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterMerchantMappings();
            RegisterTerminalMappings();
            RegisterUserMappings();
        }

        private void RegisterTerminalMappings()
        {
            CreateMap<TerminalRequest, Terminal>()
                .ForMember(m => m.Created, o => o.MapFrom((src, tgt) => tgt.Created = DateTime.UtcNow));
            CreateMap<UpdateTerminalRequest, Terminal>();

            CreateMap<Business.Entities.Terminal.TerminalSettings, Models.Terminal.TerminalSettings>().ReverseMap();
            CreateMap<Business.Entities.Terminal.TerminalBillingSettings, Models.Terminal.TerminalBillingSettings>()
                .ForMember(m => m.BillingNotificationsEmails, o => o.MapFrom(
                    (src) => src.BillingNotificationsEmails.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))).ReverseMap();
            CreateMap<Terminal, TerminalResponse>();
            CreateMap<Terminal, TerminalSummary>();
            CreateMap<ExternalSystem, ExternalSystemSummary>();

                //.ForMember(src => src.ExternalSystemType, o => o.MapFrom(src => src.Type));

            CreateMap<TerminalExternalSystem, TerminalExternalSystemDetails>();
            CreateMap<ExternalSystemRequest, TerminalExternalSystem>();
        }

        private void RegisterMerchantMappings()
        {
            CreateMap<Merchant, MerchantSummary>();
            CreateMap<Merchant, MerchantResponse>();
            CreateMap<MerchantRequest, Merchant>();
            CreateMap<UpdateMerchantRequest, Merchant>();
            CreateMap<Feature, FeatureResponse>();
            CreateMap<MerchantHistory, MerchantHistoryResponse>();
        }

        private void RegisterUserMappings()
        {
            CreateMap<UserProfileDataResponse, UserResponse>();
            CreateMap<InviteUserRequest, CreateUserRequestModel>();
        }
    }
}
