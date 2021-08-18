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

            CreateMap<BillingDealUpdateRequest, BillingDeal>();

            CreateMap<BillingDeal, BillingDealSummary>()
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(d => d.CreditCardDetails.CardOwnerName))
                .ForMember(d => d.CardExpired, o => o.MapFrom(d => d.CreditCardDetails.CardExpiration.Expired))
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CreditCardDetails.CardNumber)))
                .ForMember(d => d.Paused, o => o.MapFrom(d => d.Paused()));

            CreateMap<BillingDeal, BillingDealSummaryAdmin>()
                .ForMember(d => d.CardOwnerName, o => o.MapFrom(d => d.CreditCardDetails.CardOwnerName))
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

            CreateMap<BillingDeal, CreateTransactionRequest>()
                .ForMember(d => d.BankTransferDetails, o => o.MapFrom(s => s.BankDetails))
                .ForMember(d => d.PaymentTypeEnum, o => o.MapFrom(s => s.PaymentType));

            CreateMap<BillingDealHistory, BillingDealHistoryResponse>();

            CreateMap<BankDetails, BankTransferDetails>();
        }
    }
}
