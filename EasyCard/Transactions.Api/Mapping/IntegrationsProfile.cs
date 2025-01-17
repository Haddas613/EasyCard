﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Shared.Helpers;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
using Transactions.Api.Mapping.ValueResolvers;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Business.Entities;
using SharedIntegration = Shared.Integration;

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
            CreateMap<Business.Entities.CreditCardDetails, SharedIntegration.Models.CreditCardDetails>()
                .ForMember(m => m.CardLastFourDigits, s => s.MapFrom(src => CreditCardHelpers.GetCardLastFourDigits(src.CardNumber)))
                .ForMember(m => m.CardBin, s => s.MapFrom(src => CreditCardHelpers.GetCardBin(src.CardNumber)));

            CreateMap<Business.Entities.CreditCardDetails, Models.Transactions.CreditCardDetails>()
                .ForMember(m => m.CardNumber, s => s.MapFrom(src => src.CardNumber));

            CreateMap<PaymentTransaction, AggregatorCreateTransactionRequest>()
                .ForMember(m => m.TransactionID, s => s.MapFrom(src => src.PaymentTransactionID.ToString()))
                .ForMember(m => m.EasyCardTerminalID, s => s.MapFrom(src => src.TerminalID.ToString()))
                .ForMember(m => m.AggregatorSettings, s => s.Ignore())
                .ForMember(m => m.TransactionType, s => s.MapFrom(src => src.TransactionType))
                .ForMember(m => m.Currency, s => s.MapFrom(src => src.Currency))
                .ForMember(m => m.CardPresence, s => s.MapFrom(src => src.CardPresence))
                .ForMember(m => m.NumberOfInstallments, s => s.MapFrom(src => src.NumberOfPayments))
                .ForMember(m => m.TransactionAmount, s => s.MapFrom(src => src.TransactionAmount))
                .ForMember(m => m.InitialPaymentAmount, s => s.MapFrom(src => src.InitialPaymentAmount))
                .ForMember(m => m.TotalAmount, s => s.MapFrom(src => src.TotalAmount))
                .ForMember(m => m.InstallmentPaymentAmount, s => s.MapFrom(src => src.InstallmentPaymentAmount))
                .ForMember(m => m.TransactionDate, s => s.MapFrom(src => src.TransactionDate))
                .ForMember(m => m.CreditCardDetails, s => s.MapFrom(src => src.CreditCardDetails))
                .ForMember(m => m.DealDetails, s => s.MapFrom(src => src.DealDetails));

            CreateMap<PaymentTransaction, ProcessorCreateTransactionRequest>()
                .ForMember(m => m.PaymentTransactionID, s => s.MapFrom(src => src.PaymentTransactionID.ToString()))
                .ForMember(m => m.CreditCardToken, s => s.Ignore())
                .ForMember(m => m.EasyCardTerminalID, s => s.MapFrom(src => src.TerminalID))
                .ForMember(m => m.PinpadDeviceID, s => s.MapFrom(src => src.PinPadDeviceID))
                .ForMember(m => m.OKNumber, s => s.MapFrom(src => src.OKNumber)) // TODO: remove this
                .ForMember(m => m.BitPaymentInitiationId, s => s.MapFrom(src => src.BitTransactionDetails.BitPaymentInitiationId))
                .ForMember(m => m.BitTransactionSerialId, s => s.MapFrom(src => src.BitTransactionDetails.BitTransactionSerialId))
                .ForMember(m => m.PinPadTransactionID, s => s.Ignore());

            CreateMap<ProcessorCreateTransactionResponse, PaymentTransaction>()
                .ForMember(m => m.ProcessorResultCode, s => s.MapFrom(src => src.ResultCode));

            CreateMap<ProcessorPreCreateTransactionResponse, ProcessorCreateTransactionRequest>()
                .ForMember(m => m.PinPadTransactionID, s => s.MapFrom(src => src.PinPadTransactionID))
                .ForPath(m => m.CreditCardToken.CardNumber, s => s.MapFrom(src => src.CardNumber))
                ;

            CreateMap<ProcessorCreateTransactionResponse, CreditCardTokenDetails>();

            CreateMap<PaymentTransaction, AggregatorCommitTransactionRequest>()
                .ForMember(m => m.TransactionID, s => s.MapFrom(src => src.PaymentTransactionID.ToString()))
                .ForMember(m => m.AggregatorTransactionID, s => s.MapFrom(src => src.ClearingHouseTransactionDetails == null || src.ClearingHouseTransactionDetails.ClearingHouseTransactionID == null ? src.UpayTransactionDetails.CashieriD : src.ClearingHouseTransactionDetails.ClearingHouseTransactionID.ToString())) // TODO
                .ForMember(m => m.ConcurrencyToken, s => s.MapFrom(src => src.ClearingHouseTransactionDetails.ConcurrencyToken)) // TODO
                .ForMember(m => m.ProcessorTransactionDetails, s => s.MapFrom(src => src.ShvaTransactionDetails))
                ; // TODO

            CreateMap<PaymentTransaction, AggregatorCancelTransactionRequest>()
                .ForMember(m => m.TransactionID, s => s.MapFrom(src => src.PaymentTransactionID.ToString()))
                .ForMember(m => m.AggregatorTransactionID, s => s.MapFrom(src => src.ClearingHouseTransactionDetails.ClearingHouseTransactionID)) // TODO
                .ForMember(m => m.ConcurrencyToken, s => s.MapFrom(src => src.ClearingHouseTransactionDetails.ConcurrencyToken)); // TODO

            CreateMap<AggregatorCreateTransactionResponse, PaymentTransaction>();
            CreateMap<AggregatorCommitTransactionResponse, PaymentTransaction>();
            CreateMap<AggregatorCancelTransactionResponse, PaymentTransaction>();
            CreateMap<AggregatorTransactionResponse, PaymentTransaction>();

            CreateMap<CreditCardTokenKeyVault, CreditCardSecureDetails>();
            //CreateMap<CreditCardTokenKeyVault, PaymentTransaction>()
            //    .ForMember(d => d.OKNumber, o => o.MapFrom(d => d.OKNumber))
            //    .ForAllOtherMembers(d => d.Ignore());

            CreateMap<CreditCardSecureDetails, CreditCardSecureDetails>();

            CreateMap<Invoice, InvoicingCreateDocumentRequest>()
                 .ForMember(m => m.InvoiceID, s => s.MapFrom(src => src.InvoiceID.ToString()))
                 .ForMember(m => m.ConsumerName, s => s.MapFrom(src => src.DealDetails.ConsumerName))
                 .ForMember(m => m.ConsumerNationalID, s => s.MapFrom(src => src.DealDetails.ConsumerNationalID))
                 .ForMember(m => m.TransactionType, s => s.MapFrom(src => src.TransactionType))
                 .ForMember(d => d.PaymentDetails, o => o.MapFrom(d => d.PaymentDetails));

            CreateMap<InvoicingCreateDocumentResponse, Invoice>()
                 .ForMember(m => m.InvoiceNumber, s => s.MapFrom(src => src.DocumentNumber));

            CreateMap<SharedIntegration.ExternalSystems.NullAggregatorSettings, PaymentTransaction>()
                .ForAllMembers(d => d.Ignore());
        }
    }
}
