using AutoMapper;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions;
using Transactions.Business.Entities;

namespace Transactions.Api.Infrastructure.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterTransactionMappings();
        }

        private void RegisterTransactionMappings()
        {
            CreateMap<TransactionRequest, PaymentTransaction>();
            CreateMap<PaymentTransaction, TransactionResponse>();
            CreateMap<PaymentTransaction, TransactionSummary>();
            CreateMap<TokenRequest, CreditCardToken>();

            CreateMap<CreditCardToken, CreditCardTokenDetails>()
                .ForMember(m => m.Hash, src => src.MapFrom(f => CreditCardHelpers.GetCardHash(f.CardNumber, f.TerminalID, f.MerchantID, f.CardExpiration.ToString())));
        }
    }
}
