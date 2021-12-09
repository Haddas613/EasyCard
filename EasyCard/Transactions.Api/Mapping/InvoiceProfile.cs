using AutoMapper;
using Shared.Helpers;
using Shared.Integration.Models.PaymentDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Mapping.ValueResolvers;
using Transactions.Api.Models.Invoicing;
using Transactions.Business.Entities;
using TransactionsApi = Transactions.Api;

namespace Transactions.Api.Mapping
{
    public class InvoiceProfile : Profile
    {
        public InvoiceProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterInvoiceMappings();
        }

        private void RegisterInvoiceMappings()
        {
            CreateMap<InvoiceRequest, Invoice>()
                .ForMember(d => d.InitialPaymentAmount, s => s.MapFrom(src => src.InstallmentDetails.InitialPaymentAmount))
                .ForMember(d => d.NumberOfPayments, s => s.MapFrom(src => src.InstallmentDetails.NumberOfPayments))
                .ForMember(d => d.TotalAmount, s => s.MapFrom(src => src.InstallmentDetails.TotalAmount))
                .ForMember(d => d.InstallmentPaymentAmount, s => s.MapFrom(src => src.InstallmentDetails.InstallmentPaymentAmount));

            CreateMap<TransactionsApi.Models.Transactions.CreditCardDetails, Transactions.Business.Entities.CreditCardDetails>();

            CreateMap<Transactions.Business.Entities.CreditCardDetails, TransactionsApi.Models.Transactions.CreditCardDetails>()
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardLastFourDigitsWithPrefix(d.CardNumber)));

            CreateMap<Invoice, InvoiceSummary>()
                  .ForMember(d => d.InvoiceType, o => o.MapFrom(d => d.InvoiceDetails.InvoiceType))
                  .ForMember(d => d.ConsumerID, o => o.MapFrom(d => d.DealDetails.ConsumerID));

            CreateMap<Invoice, InvoiceSummaryAdmin>()
                  .ForMember(d => d.InvoiceType, o => o.MapFrom(d => d.InvoiceDetails.InvoiceType))
                  .ForMember(d => d.ConsumerID, o => o.MapFrom(d => d.DealDetails.ConsumerID));

            CreateMap<Invoice, InvoiceResponse>();
            CreateMap<InvoiceHistory, InvoiceHistoryResponse>();

            CreateMap<CreditCardDetails, CreditCardPaymentDetails>()
                .ForMember(d => d.СardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardLastFourDigitsWithPrefix(d.CardNumber)));

            CreateMap<BillingDeal, Invoice>();

            // TODO: specify all members and ignore rest
            CreateMap<PaymentTransaction, Invoice>()
                .ForMember(d => d.InvoiceID, o => o.Ignore())
                .ForMember(d => d.Status, o => o.Ignore())
                .ForMember(d => d.InvoiceAmount, o => o.MapFrom(d => d.TransactionAmount))
                .ForMember(d => d.TransactionType, o => o.MapFrom(d => d.TransactionType))

                //.ForPath(d => d.CreditCardDetails.Solek, o => o.MapFrom(d => d.ShvaTransactionDetails.Solek))
                //.ForPath(d => d.CreditCardDetails.CardBrand, o => o.MapFrom(d => d.CreditCardDetails.CardBrand))
                //.ForPath(d => d.CreditCardDetails.CardVendor, o => o.MapFrom(d => d.CreditCardDetails.CardVendor))
                //.ForPath(d => d.CreditCardDetails.CardNumber, o => o.MapFrom(d => d.CreditCardDetails.CardNumber))
                //.ForPath(d => d.CreditCardDetails.CardExpiration, o => o.MapFrom(d => d.CreditCardDetails.CardExpiration))
                //.ForPath(d => d.CreditCardDetails.CardOwnerName, o => o.MapFrom(d => d.CreditCardDetails.CardOwnerName))
                //.ForPath(d => d.CreditCardDetails.CardOwnerNationalID, o => o.MapFrom(d => d.CreditCardDetails.CardOwnerNationalID))
                //.ForPath(d => d.CreditCardDetails.CardReaderInput, o => o.MapFrom(d => d.CreditCardDetails.CardReaderInput))

                .ForMember(d => d.Extension, o => o.MapFrom(d => d.Extension))
                .ForMember(d => d.PaymentDetails, o => o.MapFrom<PaymentDetailsTransactionValueResolver>());
        }
    }
}
