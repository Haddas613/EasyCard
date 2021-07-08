using AutoMapper;
using Nayax;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Mapping
{
    public class PinpadProfile : Profile
    {
        public PinpadProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterTransactionMappings();
        }

        private void RegisterTransactionMappings()
        {
            CreateMap<SharedIntegration.Models.ProcessorPreCreateTransactionResponse, PaymentTransaction>()
                .ForMember(m => m.ShvaTransactionDetails, s => s.MapFrom(src => src))
                .ForMember(m => m.CreditCardDetails, s => s.MapFrom(src => src))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<NayaxCreateTransactionResponse, PaymentTransaction>()
                .ForMember(m => m.PinPadTransactionID, s => s.MapFrom(src => src.PinPadTransactionID))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<SharedIntegration.Models.ProcessorPreCreateTransactionResponse, CreditCardDetails>()
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CardNumber)))
                .ForMember(d => d.CardBin, o => o.MapFrom(d => CreditCardHelpers.GetCardBin(d.CardNumber)));

            CreateMap<ShvaTransactionDetails, SharedIntegration.Models.ProcessorCreateTransactionRequest>()
                .ForMember(m => m.LastDealShvaDetails, s => s.MapFrom(src => src));

            CreateMap<ShvaTransactionDetails, SharedIntegration.Models.Processor.ShvaTransactionDetails>();
        }
    }
}
