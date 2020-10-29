using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Extensions;
using Transactions.Api.Models.PaymentRequests;
using Transactions.Business.Entities;

namespace Transactions.Api.Mapping
{
    public class PaymentRequestProfile : Profile
    {
        public PaymentRequestProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterPaymentRequestMappings();
        }

        private void RegisterPaymentRequestMappings()
        {
            CreateMap<PaymentRequestCreate, PaymentRequest>();

            CreateMap<PaymentRequest, PaymentRequestSummary>()
                  .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus()))
                  .ForMember(d => d.ConsumerID, o => o.MapFrom(d => d.DealDetails.ConsumerID));

            CreateMap<PaymentRequest, PaymentRequestResponse>();
        }
    }
}
