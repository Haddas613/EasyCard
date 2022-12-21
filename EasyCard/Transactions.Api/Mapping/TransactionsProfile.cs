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
            //CreateMap<ShvaTransactionDetails, PaymentTransaction>()
            //    .ForMember(d => d.OKNumber, s => s.MapFrom(src => src.ShvaAuthNum));

            CreateMap<CreateTransactionRequest, PaymentTransaction>()
                   .ForMember(d => d.InitialPaymentAmount, s => s.MapFrom(src => src.InstallmentDetails.InitialPaymentAmount))
                   .ForMember(d => d.NumberOfPayments, s => s.MapFrom(src => src.InstallmentDetails.NumberOfPayments))
                   .ForMember(d => d.InstallmentPaymentAmount, s => s.MapFrom(src => src.InstallmentDetails.InstallmentPaymentAmount))
                   .ForMember(d => d.OKNumber, s => s.MapFrom(src => src.OKNumber)) // this is value entered in AuthorizationCode field on UI
                   .ForMember(d => d.ConnectionID, s => s.MapFrom(src => src.ConnectionID))
                   .ForMember(d => d.Extension, s => s.MapFrom(src => src.Extension))
                   .ForMember(d => d.PaymentIntentID, s => s.MapFrom(src => src.PaymentIntentID))
                    .ForMember(d => d.PaymentRequestID, s => s.MapFrom(src => src.PaymentRequestID))
                    .ForMember(d => d.InitialTransactionID, s => s.MapFrom(src => src.InitialJ5TransactionID))
                .ForMember(d => d.CreditCardDetails, o => o.Ignore());

            CreateMap<CreditCardSecureDetails, Business.Entities.CreditCardDetails>()
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CardNumber)))
                .ForMember(d => d.CardBin, o => o.MapFrom(d => CreditCardHelpers.GetCardBin(d.CardNumber)));

            // NOTE: this is security assignment
            CreateMap<Merchants.Business.Entities.Terminal.Terminal, PaymentTransaction>()
                .ForMember(d => d.TerminalID, o => o.MapFrom(d => d.TerminalID))
                .ForMember(d => d.TerminalTemplateID, o => o.MapFrom(d => d.TerminalTemplateID))
                .ForMember(d => d.MerchantID, o => o.MapFrom(d => d.MerchantID))
                .ForMember(d => d.WebHooksConfiguration, o => o.MapFrom(d => d.WebHooksConfiguration))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<PaymentTransaction, TransactionResponse>()
                .ForMember(d => d.AllowRefund, o => o.Ignore())
                .ForMember(d => d.AllowTransmission, o => o.Ignore())
                .ForMember(d => d.AllowTransmissionCancellation, o => o.Ignore())
                .ForMember(d => d.AllowInvoiceCreation, o => o.Ignore());

            CreateMap<PaymentTransaction, TransactionResponseAdmin>();

            CreateMap<PaymentTransaction, TransactionSummary>()
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(src => src.DealDetails.ConsumerName ?? src.CreditCardDetails.CardOwnerName))
                .ForMember(d => d.ShvaDealID, o => o.MapFrom(src => src.ShvaTransactionDetails.ShvaDealID))
                .ForMember(d => d.ConsumerExternalReference, o => o.MapFrom(src => src.DealDetails.ConsumerExternalReference))
                .ForMember(d => d.DealDescription, o => o.MapFrom(src => src.DealDetails.DealDescription))
                .ForMember(d => d.Solek, o => o.MapFrom(src => src.ShvaTransactionDetails.Solek))
                .ForMember(d => d.CardVendor, o => o.MapFrom(src => src.CreditCardDetails.CardVendor))
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CreditCardDetails.CardNumber)));

            CreateMap<PaymentTransaction, TransactionSummaryAdmin>()
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(src => src.DealDetails.ConsumerName ?? src.CreditCardDetails.CardOwnerName))
                .ForMember(d => d.ShvaDealID, o => o.MapFrom(src => src.ShvaTransactionDetails.ShvaDealID))
                .ForMember(d => d.ConsumerExternalReference, o => o.MapFrom(src => src.DealDetails.ConsumerExternalReference))
                .ForMember(d => d.DealDescription, o => o.MapFrom(src => src.DealDetails.DealDescription))
                .ForMember(d => d.Solek, o => o.MapFrom(src => src.ShvaTransactionDetails.Solek))
                .ForMember(d => d.CardVendor, o => o.MapFrom(src => src.CreditCardDetails.CardVendor))
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CreditCardDetails.CardNumber)));

            CreateMap<TransactionSummaryDb, TransactionSummary>()
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(src => src.CardOwnerName));

            CreateMap<SharedIntegration.Models.DealDetails, Business.Entities.DealDetails>()
                .ForMember(c => c.ConsumerAddress, o => o.MapFrom(src => src.ConsumerAddress))
                .ForMember(c => c.ConsumerExternalReference, o => o.MapFrom(src => src.ConsumerExternalReference));

            CreateMap<Business.Entities.DealDetails, SharedIntegration.Models.DealDetails>()
                .ForMember(c => c.ConsumerAddress, o => o.MapFrom(src => src.ConsumerAddress))
                 .ForMember(c => c.ConsumerExternalReference, o => o.MapFrom(src => src.ConsumerExternalReference));

            CreateMap<CreditCardTokenKeyVault, Business.Entities.CreditCardDetails>()
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CardNumber)))
                .ForMember(d => d.CardBin, o => o.MapFrom(d => CreditCardHelpers.GetCardBin(d.CardNumber)));

            //CreateMap<CreditCardTokenKeyVault, PaymentTransaction>()
            //    .ForMember(d => d.OKNumber, o => o.MapFrom(d => d.OKNumber))
            //    .ForAllOtherMembers(d => d.Ignore());

            CreateMap<RefundRequest, CreateTransactionRequest>();
            CreateMap<BlockCreditCardRequest, CreateTransactionRequest>();
            CreateMap<CheckCreditCardRequest, CreateTransactionRequest>();
            CreateMap<InitalBillingDealRequest, CreateTransactionRequest>();
            CreateMap<NextBillingDealRequest, CreateTransactionRequest>();

            // required for Bit
            CreateMap<PaymentRequest, CreateTransactionRequest>()
                .ForMember(d => d.PaymentIntentID, o => o.Ignore())
                .ForMember(d => d.PaymentRequestID, o => o.Ignore())
                .ForMember(d => d.TransactionAmount, o => o.MapFrom((src, target) => target.TransactionAmount == 0 ? src.PaymentRequestAmount : target.TransactionAmount));

            CreateMap<PRCreateTransactionRequest, CreateTransactionRequest>()
                .ForMember(d => d.ConnectionID, s => s.MapFrom(src => src.ConnectionID))
                .ForMember(d => d.OKNumber, s => s.MapFrom(src => src.OKNumber))
                .ForMember(d => d.TransactionAmount, o => o.MapFrom(d => d.PaymentRequestAmount)); // TODO only for user amount

            CreateMap<PaymentTransaction, CreateTransactionRequest>()
                .ForMember(d => d.OKNumber, o => o.MapFrom(d => d.ShvaTransactionDetails.ShvaAuthNum))
                .ForMember(d => d.InitialJ5TransactionID, o => o.MapFrom(d => TransactionsHelper.GetJ5transactionID(d.PaymentTransactionID, (int)d.JDealType)));

            CreateMap<Business.Entities.TransactionHistory, Models.Transactions.TransactionHistory>();

            CreateMap<Business.Entities.CreditCardTokenDetails, Business.Entities.CreditCardDetails>();

            CreateMap<SharedIntegration.Models.DealDetails, Merchants.Business.Entities.Billing.Consumer>()
                .ForMember(d => d.ConsumerID, o => o.Ignore());

            CreateMap<PaymentTransaction, TransmissionReportSummary>()
                .ForMember(d => d.ConsumerName, o => o.MapFrom(src => src.DealDetails.ConsumerName ?? src.CreditCardDetails.CardOwnerName))
                .ForMember(d => d.Date, o => o.MapFrom(src => src.UpdatedDate))
                .ForMember(d => d.TransmissionDate, o => o.MapFrom(src => src.ShvaTransactionDetails.TransmissionDate))
                .ForMember(d => d.ShvaDealID, o => o.MapFrom(src => src.ShvaTransactionDetails.ShvaDealID))
                .ForMember(d => d.ShvaTransmissionNumber, o => o.MapFrom(src => src.ShvaTransactionDetails.ShvaTransmissionNumber))
                .ForMember(d => d.Solek, o => o.MapFrom(src => src.ShvaTransactionDetails.Solek));

            CreateMap<PaymentTransaction, TransmissionReportSummaryAdmin>()
               .ForMember(d => d.ConsumerName, o => o.MapFrom(src => src.DealDetails.ConsumerName ?? src.CreditCardDetails.CardOwnerName))
               .ForMember(d => d.Date, o => o.MapFrom(src => src.UpdatedDate))
               .ForMember(d => d.TransmissionDate, o => o.MapFrom(src => src.ShvaTransactionDetails.TransmissionDate))
               .ForMember(d => d.ShvaDealID, o => o.MapFrom(src => src.ShvaTransactionDetails.ShvaDealID))
               .ForMember(d => d.ShvaTransmissionNumber, o => o.MapFrom(src => src.ShvaTransactionDetails.ShvaTransmissionNumber))
               .ForMember(d => d.Solek, o => o.MapFrom(src => src.ShvaTransactionDetails.Solek));
        }
    }
}
