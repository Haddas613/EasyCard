﻿using AutoMapper;
using IdentityServerClient;
using Merchants.Api.Models;
using Merchants.Api.Models.Merchant;
using Merchants.Api.Models.Terminal;
using Merchants.Api.Models.User;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Models.Integration;
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

            CreateMap<Terminal, TerminalResponse>();
            CreateMap<Terminal, TerminalSummary>()
                .ForMember(m => m.MerchantBusinessName, o => o.MapFrom(src => src.Merchant.BusinessName))
                .ForMember(m => m.MerchantID, o => o.MapFrom(src => src.MerchantID));
            CreateMap<ExternalSystem, ExternalSystemSummary>();

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
            CreateMap<Business.Entities.User.UserInfo, UserSummary>();
        }
    }
}
