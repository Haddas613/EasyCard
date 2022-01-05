using AutoMapper;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Extensions;
using Transactions.Api.Models.Transactions;
using Transactions.Business.Entities;
using Nayax.Converters;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Mapping
{
    public class NayaxProfile : Profile
    {
        public NayaxProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterTransactionMappings();
        }

        private void RegisterTransactionMappings()
        {
            CreateMap<Nayax.Models.NayaxValidateRequest, AggregatorCreateTransactionRequest>()
               .ForMember(m => m.TransactionAmount, s => s.MapFrom(src => src.TransactionAmount))
               .ForMember(m => m.InitialPaymentAmount, s => s.MapFrom(src => src.FirstPaymentAmount > 0 ? src.FirstPaymentAmount : src.TransactionAmount))
               .ForMember(m => m.InstallmentPaymentAmount, s => s.MapFrom(src => src.NextPaymentAmount))
               .ForMember(m => m.IsPinPad, s => s.MapFrom(src => true))
               .ForMember(m => m.JDealType, s => s.MapFrom(src => JDealTypeEnum.J4))
               .ForMember(m => m.NumberOfInstallments, s => s.MapFrom(src => src.PaymentsNumber))
               .ForMember(m => m.SpecialTransactionType, s => s.MapFrom(src => src.TranType.GetSpecialTransactionTypeFromNayax()))
               .ForMember(m => m.TransactionType, s => s.MapFrom(src => src.CreditTerms.GetTransactionTypeFromNayax()))
               .ForMember(m => m.TotalAmount, s => s.MapFrom(src => src.TransactionAmount))
               .ForAllOtherMembers(d => d.Ignore());

            CreateMap<Nayax.Models.NayaxValidateRequest, SharedIntegration.Models.CreditCardDetails>()
                .ForMember(m => m.CardLastFourDigits, s => s.MapFrom(src => CreditCardHelpers.GetCardLastFourDigits(src.MaskedPan)))
                .ForMember(m => m.CardBin, s => s.MapFrom(src => CreditCardHelpers.GetCardBin(src.MaskedPan)))
                .ForMember(m => m.CardOwnerNationalID, s => s.MapFrom(src => src.OwnerIdentityNumber))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<Nayax.Models.NayaxValidateRequest, PaymentTransaction>()
                .ForMember(d => d.TransactionAmount, s => s.MapFrom(src => src.TransactionAmount / 100m))
                .ForMember(d => d.NumberOfPayments, s => s.MapFrom(src => src.PaymentsNumber))
                .ForMember(d => d.JDealType, s => s.MapFrom(src => JDealTypeEnum.J4))
                .ForMember(d => d.InstallmentPaymentAmount, s => s.MapFrom(src => src.NextPaymentAmount / 100m))
                .ForMember(d => d.InitialPaymentAmount, s => s.MapFrom(src => src.FirstPaymentAmount / 100m))
                .ForMember(d => d.CreditCardDetails, o => o.Ignore())
                .ForMember(d => d.Currency, s => s.MapFrom(src => src.OriginalCurrency.GetCurrencyFromNayax()))
                .ForMember(d => d.TransactionType, s => s.MapFrom(src => src.CreditTerms.GetTransactionTypeFromNayax()))
                .ForMember(d => d.SpecialTransactionType, s => s.MapFrom(src => src.TranType.GetSpecialTransactionTypeFromNayax()))
                .ForAllOtherMembers(d => d.Ignore());
        }
    }
}
