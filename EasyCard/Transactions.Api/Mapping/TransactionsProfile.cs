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
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Mapping
{
    public class TransactionsProfile : Profile
    {
        public TransactionsProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterTransactionMappings();
        }

        private void RegisterTransactionMappings()
        {
            CreateMap<TransactionRequest, PaymentTransaction>();
            CreateMap<PaymentTransaction, TransactionResponse>();
            CreateMap<PaymentTransaction, TransactionSummary>();

            CreateMap<SharedIntegration.Models.DealDetails, Business.Entities.DealDetails>();
            CreateMap<Business.Entities.DealDetails, SharedIntegration.Models.DealDetails>();

            CreateMap<SharedIntegration.Models.CreditCardDetails, Business.Entities.CreditCardDetails>();
            CreateMap<SharedIntegration.Models.DealDetails, Business.Entities.DealDetails>();

            CreateMap<ProcessTransactionOptions, PaymentTransaction>()
                .ForMember(m => m.CreditCardDetails, s => s.MapFrom(src => src.CreditCardSecureDetails));
            CreateMap<CreditCardSecureDetails, Business.Entities.CreditCardDetails>()
                .ForMember(m => m.CardNumber, s => s.MapFrom(src => CreditCardHelpers.GetCardDigits(src.CardNumber)))
                .ForMember(m => m.CardBin, s => s.MapFrom(src => CreditCardHelpers.GetCardBin(src.CardNumber)));

            CreateMap<CreditCardTokenKeyVault, ProcessTransactionOptions>()
                .ForMember(m => m.TerminalID, s => s.MapFrom(src => src.TerminalID))
                .ForMember(m => m.MerchantID, s => s.MapFrom(src => src.MerchantID))
                .ForMember(m => m.CreditCardSecureDetails, s => s.MapFrom(src => new CreditCardSecureDetails { CardExpiration = src.CardExpiration, CardNumber = src.CardNumber }));
        }
    }
}
