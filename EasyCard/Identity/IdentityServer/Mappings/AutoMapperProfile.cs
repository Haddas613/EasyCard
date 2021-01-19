﻿using AutoMapper;
using IdentityServer.Models.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterMerchantMappings();
        }

        private void RegisterMerchantMappings()
        {
            CreateMap<RegisterViewModel, Merchants.Api.Client.Models.MerchantRequest>()
                .ForMember(d => d.BusinessID, o => o.MapFrom(src => src.BusinessID))
                .ForMember(d => d.BusinessName, o => o.MapFrom(src => src.BusinessName))
                .ForMember(d => d.MarketingName, o => o.MapFrom(src => src.MarketingName))
                .ForMember(d => d.ContactPerson, o => o.MapFrom(src => src.ContactName))
                .ForMember(d => d.PhoneNumber, o => o.MapFrom(src => src.PhoneNumber));
        }
    }
}