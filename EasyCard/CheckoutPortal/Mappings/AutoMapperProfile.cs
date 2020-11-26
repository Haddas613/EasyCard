using AutoMapper;
using CheckoutPortal.Models;
using Merchants.Shared.Models;
using System;
using System.Collections.Generic;
using Transactions.Api.Models.PaymentRequests;

namespace CheckoutPortal.Mappings
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
            CreateMap<CardRequest, ChargeViewModel>()
                .ForMember(d => d.RedirectUrl, o => o.MapFrom(d => d.RedirectUrl))
                .ForMember(d => d.ApiKey, o => o.MapFrom(d => d.ApiKey))
                .ForMember(d => d.PaymentRequest, o => o.MapFrom(d => d.PaymentRequest))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<PaymentRequestInfo, ChargeViewModel>()
                .ForMember(d => d.Amount, o => o.MapFrom(d => d.PaymentRequestAmount))
                .ForMember(d => d.Currency, o => o.MapFrom(d => d.Currency))
                .ForMember(d => d.Description, o => o.MapFrom(d => d.DealDetails == null ? null : d.DealDetails.DealDescription))
                .ForMember(d => d.Email, o => o.MapFrom(d => d.DealDetails == null ? null : d.DealDetails.ConsumerEmail))
                .ForMember(d => d.Name, o => o.MapFrom(d => d.CardOwnerName))
                .ForMember(d => d.NationalID, o => o.MapFrom(d => d.CardOwnerNationalID))
                .ForMember(d => d.ConsumerID, o => o.MapFrom(d => d.DealDetails == null ? null : d.DealDetails.ConsumerID))
                .ForMember(d => d.Phone, o => o.MapFrom(d => d.DealDetails == null ? null : d.DealDetails.ConsumerPhone))
                .ForAllOtherMembers(d => d.Ignore());
        }
    }
}
