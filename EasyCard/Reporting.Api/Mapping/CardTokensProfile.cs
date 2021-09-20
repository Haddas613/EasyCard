using AutoMapper;
using Merchants.Business.Entities.Terminal;
using Reporting.Shared.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;

namespace Reporting.Api.Mapping
{
    public class CardTokensProfile : Profile
    {
        public CardTokensProfile() => RegisterMappings();

        void RegisterMappings()
        {
            CreateMap<Terminal, TerminalTokensResponse>()
                .ForMember(t => t.MerchantName, o => o.MapFrom(src => src.Merchant.BusinessName));

            CreateMap<CreditCardTokenDetails, TokenTransactionsResponse>();
        }
    }
}
