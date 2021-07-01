using AutoMapper;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Business.Entities;

namespace Transactions.Api.Mapping
{
    public class ClearingHouseProfile : Profile
    {
        public ClearingHouseProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterTransactionMappings();
        }

        private void RegisterTransactionMappings()
        {
            CreateMap<ClearingHouse.ClearingHouseCreateTransactionResponse, PaymentTransaction>()
                .ForMember(m => m.ClearingHouseTransactionDetails, s => s.MapFrom(src => src))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<ClearingHouse.ClearingHouseCreateTransactionResponse, ClearingHouseTransactionDetails>();

            CreateMap<ClearingHouse.ClearingHouseTransactionResponse, PaymentTransaction>()
                .ForMember(m => m.ClearingHouseTransactionDetails, s => s.MapFrom(src => src))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<ClearingHouse.ClearingHouseTransactionResponse, ClearingHouseTransactionDetails>();

            CreateMap<ClearingHouse.ClearingHouseTerminalSettings, PaymentTransaction>()
                .ForMember(m => m.ClearingHouseTransactionDetails, s => s.MapFrom(src => src))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<ClearingHouse.ClearingHouseTerminalSettings, ClearingHouseTransactionDetails>()
               .ForMember(m => m.MerchantReference, s => s.MapFrom(src => src.MerchantReference.TryParseGuid()));

            CreateMap<ShvaTransactionDetails, ClearingHouse.Models.PaymentGatewayAdditionalDetails>()
                 .ForMember(m => m.ShvaShovarData, src => src.MapFrom(f => f.ShvaDealID))
                 .ForMember(m => m.ShvaShovarNumber, src => src.MapFrom(f => f.ShvaShovarNumber))
                 .ForMember(m => m.ShvaTransmissionNumber, src => src.MapFrom(f => f.ShvaTransmissionNumber))
                 .ForMember(m => m.Solek, src => src.MapFrom(f => f.Solek))
                 ;
        }
    }
}
