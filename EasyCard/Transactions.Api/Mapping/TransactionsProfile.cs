using AutoMapper;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Extensions;
using Transactions.Api.Models.External;
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
                .ForMember(d => d.TerminalTemplateID, o => o.MapFrom(d => d.TerminalTemplateID))
                .ForMember(d => d.MerchantID, o => o.MapFrom(d => d.MerchantID));

            CreateMap<PaymentTransaction, TransactionResponse>()
                .ForMember(d => d.AllowTransmission, o => o.MapFrom(src => src.Status == Shared.Enums.TransactionStatusEnum.AwaitingForTransmission))
                .ForMember(d => d.AllowTransmissionCancellation, o => o.MapFrom(src => src.Status == Shared.Enums.TransactionStatusEnum.AwaitingForTransmission && !src.InvoiceID.HasValue))
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus(src.JDealType)));

            CreateMap<PaymentTransaction, TransactionResponseAdmin>()
                .ForMember(d => d.AllowTransmission, o => o.MapFrom(src => src.Status == Shared.Enums.TransactionStatusEnum.AwaitingForTransmission))
                .ForMember(d => d.AllowTransmissionCancellation, o => o.MapFrom(src => src.Status == Shared.Enums.TransactionStatusEnum.AwaitingForTransmission && !src.InvoiceID.HasValue))
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus(src.JDealType)));

            CreateMap<PaymentTransaction, UpayValidateDealResult>()
               .ForMember(d => d.Amount, o => o.MapFrom(src => src.Amount))
               .ForMember(d => d.CardCompany, o => o.MapFrom(src => src.CreditCardDetails.CardVendor))
               .ForMember(d => d.CardNumber, o => o.MapFrom(src => CreditCardHelpers.GetCardLastFourDigits(src.CreditCardDetails.CardNumber)))
               .ForMember(d => d.CardOwner, o => o.MapFrom(src => src.CreditCardDetails.CardOwnerName))
               .ForMember(d => d.CardReader, o => o.MapFrom(src => src.CardPresence))
               .ForMember(d => d.CardType, o => o.MapFrom(src => src.CreditCardDetails.CardBrand))
               .ForMember(d => d.ExpiryDate, o => o.MapFrom(src => CreditCardHelpers.ParseCardExpiration(src.CreditCardDetails.CardExpiration)))
               .ForMember(d => d.ExternalID, o => o.MapFrom(src => src.PaymentRequestID))
               .ForMember(d => d.FirstPayment, o => o.MapFrom(src => src.InitialPaymentAmount))
               .ForMember(d => d.ForeignCard, o => o.MapFrom(src => CreditCardHelpers.ParseCardExpiration(src.CreditCardDetails.CardExpiration)))
               .ForMember(d => d.IDNumber, o => o.MapFrom(src => src.CreditCardDetails.CardOwnerNationalID))
               .ForMember(d => d.ManpikID, o => o.MapFrom(src => src.ShvaTransactionDetails.Solek))
               .ForMember(d => d.NumPayments, o => o.MapFrom(src => src.NumberOfPayments))
               .ForMember(d => d.OkNumber, o => o.MapFrom(src => src.ShvaTransactionDetails.ShvaAuthNum))
               .ForMember(d => d.OtherPayment, o => o.MapFrom(src => src.InitialPaymentAmount))
               .ForMember(d => d.SixDigits, o => o.MapFrom(src => CreditCardHelpers.GetCardBin(src.CreditCardDetails.CardNumber)))
               .ForMember(d => d.Terminal, o => o.MapFrom(src => src.ShvaTransactionDetails.ShvaTerminalID));
            //.ForMember(d => d.Solek, o => o.MapFrom(src => src.ShvaTransactionDetails.Solek))   //  .ForMember(d => d.CustomerConfirmation, o => o.MapFrom(src => src.CreditCardDetails.CardBrand))
            //  .ForMember(d => d.CellPhoneNotify, o => o.MapFrom(src => src.CreditCardDetails.CardBrand))








            CreateMap<PaymentTransaction, TransactionSummary>()
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(src => src.CreditCardDetails.CardOwnerName))
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus(src.JDealType)));

            CreateMap<PaymentTransaction, TransactionSummaryAdmin>()
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(src => src.CreditCardDetails.CardOwnerName))
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus(src.JDealType)))
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CreditCardDetails.CardNumber)));

            CreateMap<TransactionSummaryDb, TransactionSummary>()
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(src => src.CardOwnerName))
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus(src.JDealType)));

            CreateMap<SharedIntegration.Models.DealDetails, Business.Entities.DealDetails>()
                .ForMember(c => c.ConsumerAddress, o => o.MapFrom(src => new Address { Street = src.ConsumerAddress }));
            CreateMap<Business.Entities.DealDetails, SharedIntegration.Models.DealDetails>()
                .ForMember(c => c.ConsumerAddress, o => o.MapFrom(src => src.ConsumerAddress != null ? src.ConsumerAddress.Street : null));

            CreateMap<CreditCardTokenKeyVault, Business.Entities.CreditCardDetails>()
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CardNumber)))
                .ForMember(d => d.CardBin, o => o.MapFrom(d => CreditCardHelpers.GetCardBin(d.CardNumber)));

            CreateMap<RefundRequest, CreateTransactionRequest>();
            CreateMap<BlockCreditCardRequest, CreateTransactionRequest>();
            CreateMap<CheckCreditCardRequest, CreateTransactionRequest>();
            CreateMap<InitalBillingDealRequest, CreateTransactionRequest>();
            CreateMap<NextBillingDealRequest, CreateTransactionRequest>();
            CreateMap<PaymentRequest, CreateTransactionRequest>()
                .ForMember(d => d.TransactionAmount, o => o.MapFrom(d => d.PaymentRequestAmount));

            CreateMap<PRCreateTransactionRequest, CreateTransactionRequest>();

            CreateMap<Business.Entities.TransactionHistory, Models.Transactions.TransactionHistory>();

            CreateMap<Business.Entities.CreditCardTokenDetails, Business.Entities.CreditCardDetails>();
        }
    }
}
