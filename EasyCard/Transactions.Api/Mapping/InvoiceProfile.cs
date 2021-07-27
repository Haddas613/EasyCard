using AutoMapper;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            CreateMap<InvoiceRequest, Invoice>();

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

            // TODO: specify all members and ignore rest
            CreateMap<PaymentTransaction, Invoice>()
                .ForMember(d => d.InvoiceID, o => o.Ignore())
                .ForMember(d => d.Status, o => o.Ignore())
                .ForMember(d => d.InvoiceAmount, o => o.MapFrom(d => d.TransactionAmount))
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(d => d.CreditCardDetails.CardOwnerName))
                .ForMember(d => d.CardOwnerNationalID, o => o.MapFrom(d => d.CreditCardDetails.CardOwnerNationalID))
                .ForMember(d => d.TransactionType, o => o.MapFrom(d => d.TransactionType));
        }
    }
}
