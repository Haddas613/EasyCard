using AutoMapper;
using CheckoutPortal.Models;
using Shared.Helpers;
using Shared.Integration.Models;
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
                .ForMember(d => d.AllowPinPad, o => o.MapFrom(d => d.AllowPinPad))
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
                .ForMember(d => d.NumberOfPayments, o => o.MapFrom(d => d.NumberOfPayments))
                .ForMember(d => d.InstallmentPaymentAmount, o => o.MapFrom(d => d.InstallmentPaymentAmount))
                .ForMember(d => d.TotalAmount, o => o.MapFrom(d => d.TotalAmount))
                .ForMember(d => d.InitialPaymentAmount, o => o.MapFrom(d => d.InitialPaymentAmount))
                .ForMember(d => d.TransactionType, o => o.MapFrom(d => d.NumberOfPayments > 1 ? TransactionTypeEnum.Installments : TransactionTypeEnum.RegularDeal))
                .ForMember(d => d.IsRefund, o => o.MapFrom(src => src.IsRefund))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<Transactions.Api.Models.Checkout.TerminalCheckoutCombinedSettings, ChargeViewModel>()
                .ForMember(d => d.AllowPinPad, o => o.MapFrom(src => src.AllowPinPad))
                .ForMember(d => d.MaxInstallments, o => o.MapFrom(src => src.MaxInstallments))
                .ForMember(d => d.MinInstallments, o => o.MapFrom(src => src.MinInstallments))
                .ForMember(d => d.MaxCreditInstallments, o => o.MapFrom(src => src.MaxCreditInstallments))
                .ForMember(d => d.MinCreditInstallments, o => o.MapFrom(src => src.MinCreditInstallments))
                .ForMember(d => d.TransactionTypes, o => o.MapFrom(src => src.TransactionTypes))
                .ForMember(d => d.PinPadDevices, o => o.MapFrom(src => src.PinPadDevices))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<Transactions.Api.Models.Checkout.ConsumerInfo, ChargeViewModel>()
                .ForMember(d => d.Phone, o => o.MapFrom(d => d.ConsumerPhone))
                .ForMember(d => d.NationalID, o => o.MapFrom(d => d.ConsumerNationalID))
                .ForMember(d => d.Email, o => o.MapFrom(d => d.ConsumerEmail))
                .ForMember(d => d.Name, o => o.MapFrom(d => d.ConsumerName))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));

            CreateMap<ChargeViewModel, Transactions.Api.Models.Transactions.PRCreateTransactionRequest>()
                   .ForMember(d => d.PinPad, o => o.MapFrom(d => d.PinPad))
                   .ForMember(d => d.PinPadDeviceID, o => o.MapFrom(d => d.PinPadDeviceID));

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

            CreateMap<PaymentRequestInfo, Transactions.Api.Models.Transactions.PRCreateTransactionRequest>()
                .ForMember(d => d.PaymentRequestID, o => o.Ignore());

            CreateMap<Transactions.Api.Models.Checkout.TerminalCheckoutCombinedSettings, Transactions.Api.Models.Transactions.PRCreateTransactionRequest>();

            CreateMap<Transactions.Api.Models.Checkout.TerminalCheckoutCombinedSettings, Transactions.Api.Models.Transactions.CreateTransactionRequest>();

            CreateMap<Transactions.Api.Models.Checkout.PinPadDevice, PinPadDevice>();
        }
    }
}
