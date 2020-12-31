using AutoMapper;
using CheckoutPortal.Models;
using Merchants.Shared.Models;
using Shared.Helpers;
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

                .ForMember(d => d.Amount, o => o.MapFrom(d => d.Amount))
                .ForMember(d => d.Currency, o => o.MapFrom(d => d.Currency))
                .ForMember(d => d.Description, o => o.MapFrom(d => d.Description))
                .ForMember(d => d.Email, o => o.MapFrom(d => d.Email))
                .ForMember(d => d.Name, o => o.MapFrom(d => d.Name))
                .ForMember(d => d.NationalID, o => o.MapFrom(d => d.NationalID))
                .ForMember(d => d.ConsumerID, o => o.MapFrom(d => d.ConsumerID))
                .ForMember(d => d.Phone, o => o.MapFrom(d => d.Phone))

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

            CreateMap<ChargeViewModel, Transactions.Api.Models.Transactions.PRCreateTransactionRequest>();

            CreateMap<ChargeViewModel, Transactions.Api.Models.Transactions.CreateTransactionRequest>()
                .ForMember(d => d.TransactionAmount, o => o.MapFrom(d => d.Amount))
                .ForMember(d => d.Currency, o => o.MapFrom(d => d.Currency));

            CreateMap<ChargeViewModel, Shared.Integration.Models.CreditCardSecureDetails>()
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => string.IsNullOrWhiteSpace(d.CardNumber) ? null : d.CardNumber.Replace(" ", string.Empty)))
                .ForMember(d => d.Cvv, o => o.MapFrom(d => d.Cvv))
                .ForMember(d => d.CardExpiration, o => o.MapFrom(d => string.IsNullOrWhiteSpace(d.CardExpiration) ? null : CreditCardHelpers.ParseCardExpiration(d.CardExpiration)))
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(d => d.Name))
                .ForMember(d => d.CardOwnerNationalID, o => o.MapFrom(d => d.NationalID));

            CreateMap<ChargeViewModel, Shared.Integration.Models.DealDetails>()
                .ForMember(d => d.DealDescription, o => o.MapFrom(d => d.Description))
                .ForMember(d => d.ConsumerEmail, o => o.MapFrom(d => d.Email))
                .ForMember(d => d.ConsumerPhone, o => o.MapFrom(d => d.Phone))
                .ForMember(d => d.ConsumerID, o => o.MapFrom(d => d.ConsumerID));

            CreateMap<PaymentRequestInfo, Transactions.Api.Models.Transactions.PRCreateTransactionRequest>();
            CreateMap<Transactions.Api.Models.Checkout.TerminalCheckoutCombinedSettings, Transactions.Api.Models.Transactions.PRCreateTransactionRequest>();
            CreateMap<Transactions.Api.Models.Checkout.TerminalCheckoutCombinedSettings, Transactions.Api.Models.Transactions.CreateTransactionRequest>();
        }
    }
}
