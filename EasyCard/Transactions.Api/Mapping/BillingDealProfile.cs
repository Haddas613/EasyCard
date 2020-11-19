using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Billing;
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

            CreateMap<BillingDealUpdateRequest, BillingDeal>();

            CreateMap<BillingDeal, BillingDealSummary>()
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(d => d.CreditCardDetails.CardOwnerName))
                .ForMember(d => d.CardExpired, o => o
                    .MapFrom(d => (d.CreditCardDetails != null && d.CreditCardDetails.CardExpiration != null) ? d.CreditCardDetails.CardExpiration.Expired : default));

            CreateMap<BillingDeal, BillingDealResponse>()
                .ForMember(d => d.CardExpired, o => o
                    .MapFrom(d => (d.CreditCardDetails != null && d.CreditCardDetails.CardExpiration != null) ? d.CreditCardDetails.CardExpiration.Expired : default));
        }
    }
}
