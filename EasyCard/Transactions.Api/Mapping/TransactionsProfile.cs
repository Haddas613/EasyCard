using AutoMapper;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Extensions;
using Transactions.Api.Models.External;
using Transactions.Api.Models.Masav;
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
                   .ForMember(d => d.OKNumber, s => s.MapFrom(src => src.OKNumber))
                   .ForMember(d => d.ConnectionID, s => s.MapFrom(src => src.ConnectionID))
                .ForMember(d => d.CreditCardDetails, o => o.Ignore());

            CreateMap<CreditCardSecureDetails, Business.Entities.CreditCardDetails>()
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CardNumber)))
                .ForMember(d => d.CardBin, o => o.MapFrom(d => CreditCardHelpers.GetCardBin(d.CardNumber)));

            // NOTE: this is security assignment
            CreateMap<Merchants.Business.Entities.Terminal.Terminal, PaymentTransaction>()
                .ForMember(d => d.TerminalID, o => o.MapFrom(d => d.TerminalID))
                .ForMember(d => d.TerminalTemplateID, o => o.MapFrom(d => d.TerminalTemplateID))
                .ForMember(d => d.MerchantID, o => o.MapFrom(d => d.MerchantID))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<PaymentTransaction, TransactionResponse>()
                .ForMember(d => d.AllowTransmission, o => o.MapFrom(src => src.Status == Shared.Enums.TransactionStatusEnum.AwaitingForTransmission))
                .ForMember(d => d.AllowTransmissionCancellation, o => o.MapFrom(src => src.Status == Shared.Enums.TransactionStatusEnum.AwaitingForTransmission && !src.InvoiceID.HasValue))
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus(src.JDealType)));

            CreateMap<PaymentTransaction, TransactionResponseAdmin>()
                .ForMember(d => d.AllowTransmission, o => o.MapFrom(src => src.Status == Shared.Enums.TransactionStatusEnum.AwaitingForTransmission))
                .ForMember(d => d.AllowTransmissionCancellation, o => o.MapFrom(src => src.Status == Shared.Enums.TransactionStatusEnum.AwaitingForTransmission && !src.InvoiceID.HasValue))
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus(src.JDealType)));

            CreateMap<PaymentTransaction, TransactionSummary>()
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(src => src.CreditCardDetails.CardOwnerName))
                .ForMember(d => d.ShvaDealID, o => o.MapFrom(src => src.ShvaTransactionDetails.ShvaDealID))
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus(src.JDealType)));

            CreateMap<PaymentTransaction, TransactionSummaryAdmin>()
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(src => src.CreditCardDetails.CardOwnerName))
                .ForMember(d => d.ShvaDealID, o => o.MapFrom(src => src.ShvaTransactionDetails.ShvaDealID))
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus(src.JDealType)))
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CreditCardDetails.CardNumber)));

            CreateMap<TransactionSummaryDb, TransactionSummary>()
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(src => src.CardOwnerName))
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus(src.JDealType)));

            CreateMap<SharedIntegration.Models.DealDetails, Business.Entities.DealDetails>()
                .ForMember(c => c.ConsumerAddress, o => o.MapFrom(src => new Address { Street = src.ConsumerAddress }))
                  .ForMember(c => c.ConsumerExternalReference, o => o.MapFrom(src => src.ConsumerExternalReference));

            CreateMap<Business.Entities.DealDetails, SharedIntegration.Models.DealDetails>()
                .ForMember(c => c.ConsumerAddress, o => o.MapFrom(src => src.ConsumerAddress != null ? src.ConsumerAddress.Street : null))
                 .ForMember(c => c.ConsumerExternalReference, o => o.MapFrom(src => src.ConsumerExternalReference));

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

            CreateMap<PRCreateTransactionRequest, CreateTransactionRequest>()
                .ForMember(d => d.ConnectionID, s => s.MapFrom(src => src.ConnectionID))
                .ForMember(d => d.TransactionAmount, o => o.MapFrom(d => d.PaymentRequestAmount)); // TODO only for user amount

            CreateMap<PaymentTransaction, CreateTransactionRequest>()
                .ForMember(d => d.OKNumber, o => o.MapFrom(d => d.ShvaTransactionDetails.ShvaAuthNum))
                .ForMember(d => d.InitialJ5TransactionID, o => o.MapFrom(d => TransactionsHelper.GetJ5transactionID(d.PaymentTransactionID, (int)d.JDealType)));

            CreateMap<Business.Entities.TransactionHistory, Models.Transactions.TransactionHistory>();

            CreateMap<Business.Entities.CreditCardTokenDetails, Business.Entities.CreditCardDetails>();

            CreateMap<Business.Entities.MasavFile, MasavFileSummary>();
            CreateMap<Business.Entities.MasavFileRow, MasavFileRowSummary>();

            CreateMap<SharedIntegration.Models.DealDetails, Merchants.Business.Entities.Billing.Consumer>()
                .ForMember(d => d.ConsumerID, o => o.Ignore());
        }
    }
}
