using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;

namespace Transactions.Api.Mapping
{
 /*   public class NayaxProfile : Profile
    {
        public NayaxProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterTransactionMappings();
        }

        private void RegisterTransactionMappings()
        {
            CreateMap<Nayax.NayaxCreateTransactionResponse, PaymentTransaction>()
                .ForMember(m => m.ShvaTransactionDetails, s => s.MapFrom(src => src))
                .ForMember(m => m.CreditCardDetails, s => s.MapFrom(src => src))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<Shva.ShvaCreateTransactionResponse, ShvaTransactionDetails>();

            CreateMap<Nayax.NayaxTerminalSettings, PaymentTransaction>()
                .ForMember(m => m.ShvaTransactionDetails, s => s.MapFrom(src => src))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<Nayax.NayaxTerminalSettings, NayaxTransactionDetails>()
                 .ForMember(m => m.ShvaTerminalID, s => s.MapFrom(src => src.MerchantNumber));

            CreateMap< Nayax.NayaxCreateTransactionResponse, CreditCardDetails>()
                 .ForMember(m => m.CardVendor, s => s.MapFrom(src => src.CreditCardVendor));

        }
    }
    */
}
