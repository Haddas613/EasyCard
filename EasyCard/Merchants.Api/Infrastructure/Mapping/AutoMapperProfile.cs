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

namespace Merchants.Api.Infrastructure.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() => RegisterMappings();

        void RegisterMappings()
        {
            RegisterMerchantMappings();
            RegisterTerminalMappings();
            RegisterUserMappings();
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
            CreateMap<ExternalSystem, ExternalSystemSummary>();
            CreateMap<TerminalExternalSystem, ExternalSystemDetails>();
        }

        void RegisterMerchantMappings()
        {
            CreateMap<Merchant, MerchantSummary>();
            CreateMap<Merchant, MerchantResponse>();
            CreateMap<MerchantRequest, Merchant>()
                .ForMember(m => m.Created, o => o.MapFrom((src, tgt) => tgt.Created = DateTime.UtcNow));
            CreateMap<UpdateMerchantRequest, Merchant>();
            CreateMap<Feature, FeatureResponse>();
        }

        void RegisterUserMappings()
        {
            CreateMap<UserProfileDataResponse, UserResponse>();
            CreateMap<UserRequest, CreateUserRequestModel>();
        }
    }
}
