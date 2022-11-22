using AutoMapper;
using Shared.Helpers;
using Shared.Integration.Models.PaymentDetails;
using Shared.Integration.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Mapping.ValueResolvers;
using Transactions.Api.Models.Invoicing;
using Transactions.Business.Entities;
using SharedIntegration = Shared.Integration;
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

            CreateMap<Invoice, SharedIntegration.Models.Invoicing.InvoicingCancelDocumentRequest>();

            CreateMap<Invoice, InvoiceSummary>()
                  .ForMember(d => d.CardOwnerName, o => o.MapFrom(d => d.DealDetails.ConsumerName))
                  .ForMember(d => d.InvoiceType, o => o.MapFrom(d => InvoiceEnumsResource.ResourceManager.GetString(d.InvoiceDetails.InvoiceType.ToString(), new CultureInfo("he"))))
                  .ForMember(d => d.Status, o => o.MapFrom(d => InvoiceStatusResource.ResourceManager.GetString(d.Status.ToString(), new CultureInfo("he"))))
                  .ForMember(d => d.ConsumerID, o => o.MapFrom(d => d.DealDetails.ConsumerID));

            CreateMap<Invoice, InvoiceSummaryAdmin>()
                  .ForMember(d => d.CardOwnerName, o => o.MapFrom(d => d.DealDetails.ConsumerName))
                  .ForMember(d => d.InvoiceType, o => o.MapFrom(d => InvoiceEnumsResource.ResourceManager.GetString(d.InvoiceDetails.InvoiceType.ToString(), new CultureInfo("he"))))
                  .ForMember(d => d.ConsumerID, o => o.MapFrom(d => d.DealDetails.ConsumerID));

            CreateMap<Invoice, InvoiceResponse>();
            CreateMap<InvoiceHistory, InvoiceHistoryResponse>();

            CreateMap<CreditCardDetails, CreditCardPaymentDetails>()
                .ForMember(d => d.СardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardLastFourDigitsWithPrefix(d.CardNumber)));

            CreateMap<BillingDeal, Invoice>();

            CreateMap<UpdateInvoiceRequest, Invoice>();
            // TODO: specify all members and ignore rest
            CreateMap<PaymentTransaction, Invoice>()
                .ForMember(d => d.InvoiceID, o => o.Ignore())
                .ForMember(d => d.Status, o => o.Ignore())
                .ForMember(d => d.InvoiceAmount, o => o.MapFrom(d => d.TransactionAmount))
                .ForMember(d => d.TransactionType, o => o.MapFrom(d => d.TransactionType))
                .ForMember(d => d.Extension, o => o.MapFrom(d => d.Extension))
                .ForMember(d => d.PaymentDetails, o => o.MapFrom<PaymentDetailsTransactionValueResolver>());

            CreateMap<Invoice, InvoiceExcelSummary>()
                  .ForMember(d => d.CardOwnerName, o => o.MapFrom(d => d.DealDetails.ConsumerName))
                   .ForMember(d => d.InvoiceType, o => o.MapFrom(d => InvoiceEnumsResource.ResourceManager.GetString(d.InvoiceDetails.InvoiceType.ToString(), new CultureInfo("he"))))
                  .ForMember(d => d.Status,o => o.MapFrom(d => InvoiceStatusResource.ResourceManager.GetString(d.InvoiceDetails.InvoiceType.ToString(), new CultureInfo("he"))))
                  .ForMember(d => d.AmountWithVat, o => o.MapFrom(d => d.InvoiceAmount))
                  .ForMember(d => d.AmountWithoutVat, o => o.MapFrom(d => d.InvoiceAmount - d.VATTotal));
        }
    }
}
