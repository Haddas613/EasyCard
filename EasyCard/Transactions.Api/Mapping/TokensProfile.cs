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
    public class TokensProfile : Profile
    {
        public TokensProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterTransactionMappings();
        }

        private void RegisterTransactionMappings()
        {
            CreateMap<TokenRequest, CreditCardTokenKeyVault>();

            CreateMap<TokenRequest, CreditCardTokenDetails>()
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CardNumber)));

            CreateMap<CreditCardTokenDetails, CreditCardTokenSummary>();

            CreateMap<CreateTransactionRequest, TokenRequest>()
                .ForMember(d => d.TerminalID, o => o.MapFrom(d => d.TerminalID))
                .ForMember(d => d.ConsumerID, o => o.MapFrom(d => d.DealDetails.ConsumerID))
                .ForMember(d => d.ConsumerEmail, o => o.MapFrom(d => d.DealDetails.ConsumerEmail))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<CreditCardSecureDetails, TokenRequest>();
        }
    }
}
