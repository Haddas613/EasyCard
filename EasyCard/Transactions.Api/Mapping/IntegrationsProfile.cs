using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Shared.Helpers;
using Shared.Integration.Models;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Business.Entities;
using IntegrationCreditCardDetails = Shared.Integration.Models.CreditCardDetails;
using SharedDealDetails = Shared.Integration.Models.DealDetails;

namespace Transactions.Api.Mapping
{
    public class IntegrationsProfile : Profile
    {
        public IntegrationsProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterTransactionMappings();
        }

        private void RegisterTransactionMappings()
        {
            CreateMap<IntegrationCreditCardDetails, Business.Entities.CreditCardDetails>();
            CreateMap<SharedDealDetails, Business.Entities.DealDetails>();
            CreateMap<PaymentTransaction, AggregatorCreateTransactionRequest>()
                .ForMember(m => m.TransactionID, s => s.MapFrom(src => src.PaymentTransactionID.ToString()))
                .ForMember(m => m.EasyCardTerminalID, s => s.MapFrom(src => src.TerminalID.ToString()))
                .ForMember(m => m.AggregatorTerminalID, s => s.MapFrom(src => src.AggregatorTerminalID))
                .ForMember(m => m.ProcessorTerminalID, s => s.MapFrom(src => src.ProcessorTerminalID))
                .ForMember(m => m.AggregatorSettings, s => s.Ignore())
                .ForMember(m => m.TransactionType, s => s.MapFrom(src => src.TransactionType))
                .ForMember(m => m.Currency, s => s.MapFrom(src => src.Currency))
                .ForMember(m => m.CardPresence, s => s.MapFrom(src => src.CardPresence))
                .ForMember(m => m.NumberOfInstallments, s => s.MapFrom(src => src.NumberOfPayments))
                .ForMember(m => m.CurrentInstallment, s => s.MapFrom(src => src.CurrentInstallment))
                .ForMember(m => m.TransactionAmount, s => s.MapFrom(src => src.TransactionAmount))
                .ForMember(m => m.InitialPaymentAmount, s => s.MapFrom(src => src.InitialPaymentAmount))
                .ForMember(m => m.TotalAmount, s => s.MapFrom(src => src.TotalAmount))
                .ForMember(m => m.InstallmentPaymentAmount, s => s.MapFrom(src => src.InstallmentPaymentAmount))
                .ForMember(m => m.TransactionDate, s => s.MapFrom(src => src.TransactionDate))
                .ForMember(m => m.CreditCardDetails, s => s.MapFrom(src => src.CreditCardDetails))
                .ForMember(m => m.DealDetails, s => s.MapFrom(src => src.DealDetails));

            CreateMap<PaymentTransaction, ProcessorTransactionRequest>();
            CreateMap<AggregatorCreateTransactionResponse, PaymentTransaction>();
            CreateMap<ProcessorTransactionResponse, PaymentTransaction>();
            CreateMap<PaymentTransaction, AggregatorCommitTransactionRequest>();
            CreateMap<AggregatorCommitTransactionResponse, PaymentTransaction>();
        }
    }
}
