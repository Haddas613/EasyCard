using AutoMapper;
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
            CreateMap<BillingDealRequest, BillingDeal>();
            CreateMap<BillingDealInvoiceOnlyRequest, BillingDeal>()
                .ForMember(d => d.InvoiceOnly, o => o.MapFrom(d => true))
                .ForMember(d => d.IssueInvoice, o => o.MapFrom(d => true));

            CreateMap<BillingDealUpdateRequest, BillingDeal>()
                .ForMember(d => d.DealDetails, o => o.MapFrom(d => d.DealDetails))
                .ForMember(d => d.TransactionAmount, o => o.MapFrom(d => d.TransactionAmount))
                .ForMember(d => d.BillingSchedule, o => o.MapFrom(d => d.BillingSchedule))
                .ForMember(d => d.VATRate, o => o.MapFrom(d => d.VATRate))
                .ForMember(d => d.VATTotal, o => o.MapFrom(d => d.VATTotal))
                .ForMember(d => d.NetTotal, o => o.MapFrom(d => d.NetTotal))
                .ForMember(d => d.BankDetails, o => o.MapFrom(d => d.BankDetails))
                .ForMember(d => d.CreditCardToken, o => o.MapFrom(d => d.CreditCardToken))
                .ForMember(d => d.Currency, o => o.MapFrom(d => d.Currency))
                .ForMember(d => d.InvoiceDetails, o => o.MapFrom(d => d.InvoiceDetails))
                .ForMember(d => d.IssueInvoice, o => o.MapFrom(d => d.IssueInvoice))
                .ForAllOtherMembers(o => o.Ignore());

            CreateMap<BillingDealInvoiceOnlyUpdateRequest, BillingDeal>()
                .ForMember(d => d.DealDetails, o => o.MapFrom(d => d.DealDetails))
                .ForMember(d => d.TransactionAmount, o => o.MapFrom(d => d.TransactionAmount))
                .ForMember(d => d.BillingSchedule, o => o.MapFrom(d => d.BillingSchedule))
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
                .ForMember(d => d.CardExpired, o => o.MapFrom(d => d.CreditCardDetails.CardExpiration.Expired))
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CreditCardDetails.CardNumber)))
                .ForMember(d => d.Paused, o => o.MapFrom(d => d.Paused()));

            CreateMap<BillingDeal, BillingDealSummaryAdmin>()
                .ForMember(d => d.ConsumerName, o => o.MapFrom(d => d.DealDetails.ConsumerName))
                .ForMember(d => d.CardExpired, o => o.MapFrom(d => d.CreditCardDetails.CardExpiration.Expired))
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CreditCardDetails.CardNumber)))
                .ForMember(d => d.Paused, o => o.MapFrom(d => d.Paused()));

            CreateMap<FutureBilling, FutureBillingSummary>()
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(d => d.CreditCardDetails.CardOwnerName))
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CreditCardDetails.CardNumber)));

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
