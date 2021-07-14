﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Mapping
{
    public class ShvaProfile : Profile
    {
        public ShvaProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterTransactionMappings();
        }

        private void RegisterTransactionMappings()
        {
            CreateMap<Shva.ShvaCreateTransactionResponse, PaymentTransaction>()
                .ForMember(m => m.ShvaTransactionDetails, s => s.MapFrom(src => src))
                .ForMember(m => m.CreditCardDetails, s => s.MapFrom(src => src))
                .ForMember(m => m.ProcessorResultCode, s => s.MapFrom(src => src.ResultCode))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<Shva.ShvaCreateTransactionResponse, ShvaTransactionDetails>()
                 .ForMember(m => m.ShvaAuthNum, s => s.MapFrom(src => src.AuthNum));

            CreateMap<Shva.ShvaTerminalSettings, PaymentTransaction>()
                .ForMember(m => m.ShvaTransactionDetails, s => s.MapFrom(src => src))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<Shva.ShvaTerminalSettings, ShvaTransactionDetails>()
                 .ForMember(m => m.ShvaTerminalID, s => s.MapFrom(src => src.MerchantNumber));

            CreateMap<Shva.ShvaCreateTransactionResponse, CreditCardDetails>()
                 .ForMember(m => m.CardVendor, s => s.MapFrom(src => src.CreditCardVendor))
             .ForMember(m => m.CardBrand, s => s.MapFrom(src => src.Brand));

            // Token (initial deal)

            CreateMap<Shva.ShvaCreateTransactionResponse, CreditCardTokenDetails>()
                .ForMember(m => m.ShvaInitialTransactionDetails, s => s.MapFrom(src => src))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<Shva.ShvaCreateTransactionResponse, ShvaInitialTransactionDetails>();

            CreateMap<ShvaInitialTransactionDetails, Shva.Models.InitDealResultModel>()
                 .ForMember(m => m.OriginalAuthNum, s => s.MapFrom(src => src.AuthNum))
                 .ForMember(m => m.OriginalAuthSolekNum, s => s.MapFrom(src => src.AuthSolekNum))
                 .ForMember(m => m.OriginalUid, s => s.MapFrom(src => src.ShvaDealID))
                 .ForMember(m => m.OriginalTranDateTime, s => s.MapFrom(src => src.ShvaTransactionDate))
                 .ForAllOtherMembers(d => d.Ignore());

            CreateMap<ShvaTransactionDetails, SharedIntegration.Models.Processor.ShvaTransactionDetails>();
        }
    }
}
