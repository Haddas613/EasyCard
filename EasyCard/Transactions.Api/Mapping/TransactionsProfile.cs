﻿using AutoMapper;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Extensions;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Business.Entities;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Mapping
{
    public class TransactionsProfile : Profile
    {
        public TransactionsProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterTransactionMappings();
        }

        private void RegisterTransactionMappings()
        {
            CreateMap<CreateTransactionRequest, PaymentTransaction>()
                   .ForMember(d => d.InitialPaymentAmount, s => s.MapFrom(src => src.InstallmentDetails.InitialPaymentAmount))
                   .ForMember(d => d.NumberOfPayments, s => s.MapFrom(src => src.InstallmentDetails.NumberOfPayments))
                   .ForMember(d => d.InstallmentPaymentAmount, s => s.MapFrom(src => src.InstallmentDetails.InstallmentPaymentAmount))
                .ForMember(d => d.CreditCardDetails, o => o.Ignore());

            CreateMap<CreditCardSecureDetails, Business.Entities.CreditCardDetails>()
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CardNumber)))
                .ForMember(d => d.CardBin, o => o.MapFrom(d => CreditCardHelpers.GetCardBin(d.CardNumber)));

            // NOTE: this is security assignment
            CreateMap<Merchants.Business.Entities.Terminal.Terminal, PaymentTransaction>()
                .ForMember(d => d.TerminalID, o => o.MapFrom(d => d.TerminalID))
                .ForMember(d => d.MerchantID, o => o.MapFrom(d => d.MerchantID));

            CreateMap<PaymentTransaction, TransactionResponse>()
                .ForMember(d => d.AllowTransmission, o => o.MapFrom(src => src.Status == Shared.Enums.TransactionStatusEnum.CommitedByAggregator))
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus()));

            CreateMap<PaymentTransaction, TransactionSummary>()
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(src => src.CreditCardDetails.CardOwnerName))
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus()));

            CreateMap<TransactionSummaryDb, TransactionSummary>()
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(src => src.CardOwnerName))
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus()));

            CreateMap<SharedIntegration.Models.DealDetails, Business.Entities.DealDetails>();
            CreateMap<Business.Entities.DealDetails, SharedIntegration.Models.DealDetails>();

            CreateMap<CreditCardTokenKeyVault, Business.Entities.CreditCardDetails>()
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CardNumber)))
                .ForMember(d => d.CardBin, o => o.MapFrom(d => CreditCardHelpers.GetCardBin(d.CardNumber)));

            CreateMap<SharedIntegration.Models.DealDetails, Business.Entities.DealDetails>();

            CreateMap<RefundRequest, CreateTransactionRequest>();
            CreateMap<BlockCreditCardRequest, CreateTransactionRequest>();
            CreateMap<CheckCreditCardRequest, CreateTransactionRequest>();
            CreateMap<InitalBillingDealRequest, CreateTransactionRequest>();
            CreateMap<NextBillingDealRequest, CreateTransactionRequest>();
            CreateMap<PaymentRequest, CreateTransactionRequest>();
            CreateMap<PRCreateTransactionRequest, CreateTransactionRequest>();

            CreateMap<Business.Entities.TransactionHistory, Models.Transactions.TransactionHistory>();

            CreateMap<Business.Entities.CreditCardTokenDetails, Business.Entities.CreditCardDetails>();
        }
    }
}
