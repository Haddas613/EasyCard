using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Extensions;
using Transactions.Api.Models.PaymentRequests;
using Transactions.Business.Entities;
using SharedIntegration = Shared.Integration;

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
            CreateMap<PaymentRequestCreate, PaymentRequest>()
                .ForMember(d => d.InitialPaymentAmount, s => s.MapFrom(src => src.InstallmentDetails.InitialPaymentAmount))
                .ForMember(d => d.NumberOfPayments, s => s.MapFrom(src => src.InstallmentDetails.NumberOfPayments))
                .ForMember(d => d.TotalAmount, s => s.MapFrom(src => src.InstallmentDetails.TotalAmount))
                .ForMember(d => d.InstallmentPaymentAmount, s => s.MapFrom(src => src.InstallmentDetails.InstallmentPaymentAmount));

            CreateMap<PaymentRequest, PaymentRequestSummary>()
                .ForMember(d => d.ConsumerEmail, o => o.MapFrom(d => d.DealDetails.ConsumerEmail))
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus(src.DueDate)))
                .ForMember(d => d.ConsumerID, o => o.MapFrom(d => d.DealDetails.ConsumerID));

            CreateMap<PaymentRequest, PaymentRequestSummaryAdmin>()
                .ForMember(d => d.ConsumerEmail, o => o.MapFrom(d => d.DealDetails.ConsumerEmail))
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus(src.DueDate)))
                .ForMember(d => d.ConsumerID, o => o.MapFrom(d => d.DealDetails.ConsumerID));

            CreateMap<PaymentRequest, PaymentRequestResponse>()
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus(src.DueDate)))
                .ForMember(d => d.InitialPaymentAmount, s => s.MapFrom(src => src.InitialPaymentAmount))
                .ForMember(d => d.NumberOfPayments, s => s.MapFrom(src => src.NumberOfPayments))
                .ForMember(d => d.TotalAmount, s => s.MapFrom(src => src.TotalAmount))
                .ForMember(d => d.InstallmentPaymentAmount, s => s.MapFrom(src => src.InstallmentPaymentAmount));

            CreateMap<PaymentRequest, PaymentRequestInfo>()
                .ForMember(d => d.QuickStatus, o => o.MapFrom(src => src.Status.GetQuickStatus(src.DueDate)));

            CreateMap<PaymentRequestHistory, PaymentRequestHistorySummary>();
        }
    }
}
