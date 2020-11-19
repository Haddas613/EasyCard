using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Invoicing;
using Transactions.Business.Entities;

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

            CreateMap<Invoice, InvoiceSummary>()
                  .ForMember(d => d.ConsumerID, o => o.MapFrom(d => d.DealDetails.ConsumerID));

            CreateMap<Invoice, InvoiceResponse>();
        }
    }
}
