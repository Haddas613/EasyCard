using AutoMapper;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.External;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Business.Entities;

namespace Transactions.Api.Mapping
{
    public class UpayProfile : Profile
    {
        public UpayProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterTransactionMappings();
        }

        private void RegisterTransactionMappings()
        {
            CreateMap<Upay.UpayCreateTransactionResponse, PaymentTransaction>()
                .ForMember(m => m.UpayTransactionDetails, s => s.MapFrom(src => src))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<Upay.UpayCreateTransactionResponse, UpayTransactionDetails>();

            CreateMap<Upay.UpayTransactionResponse, PaymentTransaction>()
                .ForMember(m => m.UpayTransactionDetails, s => s.MapFrom(src => src))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<Upay.UpayTransactionResponse, UpayTransactionDetails>();

            CreateMap<Upay.UpayTerminalSettings, PaymentTransaction>()
                .ForMember(m => m.UpayTransactionDetails, s => s.MapFrom(src => src))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<Upay.UpayTerminalSettings, UpayTransactionDetails>();

            CreateMap<ShvaTransactionDetails, Upay.Models.PaymentGatewayAdditionalDetails>()
                 .ForMember(m => m.ShvaShovarData, src => src.MapFrom(f => f.ShvaDealID))
                 .ForMember(m => m.ShvaShovarNumber, src => src.MapFrom(f => f.ShvaShovarNumber))
                 .ForMember(m => m.ShvaAuthNum, src => src.MapFrom(f => f.ShvaAuthNum))
                 .ForMember(m => m.ShvaTransmissionNumber, src => src.MapFrom(f => f.ShvaTransmissionNumber));

            CreateMap<PaymentTransaction, UpayValidateDealResult>()
               .ForMember(d => d.Amount, o => o.MapFrom(src => src.Amount))
               .ForMember(d => d.CardCompany, o => o.MapFrom(src => src.CreditCardDetails.CardVendor))
               .ForMember(d => d.CardNumber, o => o.MapFrom(src => CreditCardHelpers.GetCardLastFourDigits(src.CreditCardDetails.CardNumber)))
               .ForMember(d => d.CardOwner, o => o.MapFrom(src => src.CreditCardDetails.CardOwnerName))
               .ForMember(d => d.CardReader, o => o.MapFrom(src => src.CardPresence))
               .ForMember(d => d.CardType, o => o.MapFrom(src => src.CreditCardDetails.CardBrand))
               .ForMember(d => d.ExpiryDate, o => o.MapFrom(src => CreditCardHelpers.ParseCardExpiration(src.CreditCardDetails.CardExpiration)))
               .ForMember(d => d.ExternalID, o => o.MapFrom(src => src.PaymentRequestID))
               .ForMember(d => d.FirstPayment, o => o.MapFrom(src => src.InitialPaymentAmount))
               .ForMember(d => d.ForeignCard, o => o.MapFrom(src => CreditCardHelpers.ParseCardExpiration(src.CreditCardDetails.CardExpiration)))
               .ForMember(d => d.IDNumber, o => o.MapFrom(src => src.CreditCardDetails.CardOwnerNationalID))
               .ForMember(d => d.ManpikID, o => o.MapFrom(src => src.ShvaTransactionDetails.Solek))
               .ForMember(d => d.NumPayments, o => o.MapFrom(src => src.NumberOfPayments))
               .ForMember(d => d.OkNumber, o => o.MapFrom(src => src.ShvaTransactionDetails.ShvaAuthNum))
               .ForMember(d => d.OtherPayment, o => o.MapFrom(src => src.InitialPaymentAmount))
               .ForMember(d => d.SixDigits, o => o.MapFrom(src => CreditCardHelpers.GetCardBin(src.CreditCardDetails.CardNumber)))
               .ForMember(d => d.Terminal, o => o.MapFrom(src => src.ShvaTransactionDetails.ShvaTerminalID));
        }
    }
}
