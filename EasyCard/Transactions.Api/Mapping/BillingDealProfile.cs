using AutoMapper;
using Merchants.Business.Entities.Billing;
using Shared.Helpers;
using Shared.Integration.Models.PaymentDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Billing;
using Transactions.Api.Models.Transactions;
using Transactions.Business.Entities;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Mapping
{
    public class BillingDealProfile : Profile
    {
        public BillingDealProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterBillingDealMappings();
        }

        private void RegisterBillingDealMappings()
        {
            CreateMap<BillingDealRequest, BillingDeal>()
                .ForMember(d => d.CreditCardToken, o => o.Ignore())
                .ForMember(d => d.BillingSchedule, o => o.Ignore());

            CreateMap<BillingDealInvoiceOnlyRequest, BillingDeal>()
                .ForMember(d => d.InvoiceOnly, o => o.MapFrom(d => true))
                .ForMember(d => d.BillingSchedule, o => o.Ignore())
                .ForMember(d => d.IssueInvoice, o => o.MapFrom(d => true));

            CreateMap<BillingDeal, BillingDealUpdateRequest>()
               .ForMember(d => d.DealDetails, o => o.MapFrom(d => d.DealDetails))
               .ForMember(d => d.BankDetails, o => o.Ignore())
               .ForAllOtherMembers(o => o.Ignore());

            CreateMap<Consumer, BillingDealUpdateRequest>()
                            .ForPath(d => d.DealDetails.ConsumerName, src => src.MapFrom(c => c.ConsumerName))
                            .ForPath(d => d.DealDetails.ConsumerEmail, src => src.MapFrom(c => c.ConsumerEmail))
                             .ForPath(d => d.DealDetails.ConsumerAddress, src => src.MapFrom(c => c.ConsumerAddress))
                             .ForPath(d => d.DealDetails.ConsumerNationalID, src => src.MapFrom(c => c.ConsumerNationalID))
                             .ForPath(d => d.DealDetails.ConsumerPhone, src => src.MapFrom(c => c.ConsumerPhone))
                            ;

            CreateMap<BillingDealUpdateRequest, BillingDeal>()
    .ForMember(d => d.DealDetails, o => o.MapFrom(d => d.DealDetails))
    .ForMember(d => d.TransactionAmount, o => o.MapFrom(d => d.TransactionAmount))
    .ForMember(d => d.VATRate, o => o.MapFrom(d => d.VATRate))
    .ForMember(d => d.VATTotal, o => o.MapFrom(d => d.VATTotal))
    .ForMember(d => d.NetTotal, o => o.MapFrom(d => d.NetTotal))
    .ForMember(d => d.BankDetails, o => o.MapFrom(d => d.BankDetails))
    .ForMember(d => d.Currency, o => o.MapFrom(d => d.Currency))
    .ForMember(d => d.InvoiceDetails, o => o.MapFrom(d => d.InvoiceDetails))
    .ForMember(d => d.IssueInvoice, o => o.MapFrom(d => d.IssueInvoice))
    .ForAllOtherMembers(o => o.Ignore());

            CreateMap<BillingDealInvoiceOnlyUpdateRequest, BillingDeal>()
                .ForMember(d => d.DealDetails, o => o.MapFrom(d => d.DealDetails))
                .ForMember(d => d.TransactionAmount, o => o.MapFrom(d => d.TransactionAmount))
                .ForMember(d => d.VATRate, o => o.MapFrom(d => d.VATRate))
                .ForMember(d => d.VATTotal, o => o.MapFrom(d => d.VATTotal))
                .ForMember(d => d.Currency, o => o.MapFrom(d => d.Currency))
                .ForMember(d => d.InvoiceDetails, o => o.MapFrom(d => d.InvoiceDetails))
                .ForMember(d => d.InvoiceOnly, o => o.MapFrom(d => true))
                .ForMember(d => d.IssueInvoice, o => o.MapFrom(d => true))
                .ForMember(d => d.PaymentDetails, o => o.MapFrom(d => d.PaymentDetails))
                .ForAllOtherMembers(o => o.Ignore());

            CreateMap<BillingDeal, BillingDealSummary>()
                .ForMember(d => d.ConsumerName, o => o.MapFrom(d => d.DealDetails.ConsumerName))
                .ForMember(d => d.ConsumerExternalReference, o => o.MapFrom(d => d.DealDetails.ConsumerExternalReference))
                .ForMember(d => d.CardExpired, o => o.MapFrom(d => d.CreditCardDetails.CardExpiration.Expired))
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CreditCardDetails.CardNumber)))
                .ForMember(d => d.Paused, o => o.MapFrom(d => d.Paused()));

            CreateMap<BillingDeal, BillingDealSummaryAdmin>()
                .ForMember(d => d.ConsumerName, o => o.MapFrom(d => d.DealDetails.ConsumerName))
                .ForMember(d => d.ConsumerExternalReference, o => o.MapFrom(d => d.DealDetails.ConsumerExternalReference))
                .ForMember(d => d.CardExpired, o => o.MapFrom(d => d.CreditCardDetails.CardExpiration.Expired))
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CreditCardDetails.CardNumber)))
                .ForMember(d => d.Paused, o => o.MapFrom(d => d.Paused()));

            CreateMap<BillingDeal, BillingDealExcelSummary>()
               .ForMember(d => d.ConsumerName, o => o.MapFrom(d => d.DealDetails.ConsumerName))
               .ForMember(d => d.ConsumerExternalReference, o => o.MapFrom(d => d.DealDetails.ConsumerExternalReference))
               .ForMember(d => d.CardExpired, o => o.MapFrom(d => d.CreditCardDetails.CardExpiration.Expired))
               .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CreditCardDetails.CardNumber)))
               .ForMember(d => d.Paused, o => o.MapFrom(d => d.Paused()));

            CreateMap<FutureBilling, FutureBillingSummary>();

            CreateMap<BillingDeal, BillingDealResponse>()
                .ForMember(d => d.CardExpired, o => o
                    .MapFrom(d => (d.CreditCardDetails != null && d.CreditCardDetails.CardExpiration != null) ? d.CreditCardDetails.CardExpiration.Expired : default))
                .ForMember(d => d.Paused, o => o.MapFrom(d => d.Paused()));

            CreateMap<BillingDeal, PaymentRequest>()
                .ForMember(d => d.BillingDealID, o => o.MapFrom(e => e.BillingDealID))
                .ForMember(d => d.Amount, o => o.Ignore())
                .ForMember(d => d.InitialPaymentAmount, o => o.Ignore())
                .ForMember(d => d.InstallmentPaymentAmount, o => o.Ignore())
                .ForMember(d => d.PaymentRequestAmount, o => o.Ignore())
                .ForMember(d => d.TotalAmount, o => o.Ignore())
                .ForMember(d => d.UserAmount, o => o.MapFrom(e => false));

            CreateMap<BillingDeal, CreateTransactionRequest>()
                .ForMember(d => d.BankTransferDetails, o => o.MapFrom(s => s.BankDetails))
                .ForMember(d => d.PaymentTypeEnum, o => o.MapFrom(s => s.PaymentType));

            CreateMap<BillingDealHistory, BillingDealHistoryResponse>();

            CreateMap<BankDetails, BankTransferDetails>();
        }
    }
}
