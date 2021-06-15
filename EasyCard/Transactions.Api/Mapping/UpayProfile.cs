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

            /*CreateMap<ShvaTransactionDetails, ClearingHouse.Models.PaymentGatewayAdditionalDetails>()
                 .ForMember(m => m.ShvaShovarData, src => src.MapFrom(f => f.ShvaDealID))
                 .ForMember(m => m.ShvaShovarNumber, src => src.MapFrom(f => f.ShvaShovarNumber))
                 .ForMember(m => m.ShvaTransmissionNumber, src => src.MapFrom(f => f.ShvaTransmissionNumber));
                 */
        }
    }
}
