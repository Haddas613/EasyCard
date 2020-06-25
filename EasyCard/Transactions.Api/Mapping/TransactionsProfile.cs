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
           /*CreateMap<PaymentTransaction, CreateTransactionRequest>()
                .ForMember(m => m.InstallmentDetails, s => s.MapFrom(src => src))
                .ForAllOtherMembers(d => d.Ignore());
            */
            CreateMap<CreateTransactionRequest, PaymentTransaction>()
                   .ForMember(d => d.InitialPaymentAmount, s => s.MapFrom(src => src.InstallmentDetails.InitialPaymentAmount))
                   .ForMember(d => d.NumberOfPayments, s => s.MapFrom(src => src.InstallmentDetails.NumberOfPayments))
                   .ForMember(d => d.InstallmentPaymentAmount, s => s.MapFrom(src => src.InstallmentDetails.InstallmentPaymentAmount))
                .ForMember(d => d.CreditCardDetails, o => o.Ignore()
                );

            CreateMap<CreditCardSecureDetails, Business.Entities.CreditCardDetails>()
                .ForMember(d => d.CardNumber, o => o.MapFrom(d => CreditCardHelpers.GetCardDigits(d.CardNumber)))
                .ForMember(d => d.CardBin, o => o.MapFrom(d => CreditCardHelpers.GetCardBin(d.CardNumber)));

            CreateMap<Merchants.Business.Entities.Terminal.Terminal, PaymentTransaction>()
                .ForMember(d => d.TerminalID, o => o.MapFrom(d => d.TerminalID))
                .ForMember(d => d.MerchantID, o => o.MapFrom(d => d.MerchantID));

            CreateMap<PaymentTransaction, TransactionResponse>();
            CreateMap<PaymentTransaction, TransactionSummary>();

            CreateMap<SharedIntegration.Models.DealDetails, Business.Entities.DealDetails>();
            CreateMap<Business.Entities.DealDetails, SharedIntegration.Models.DealDetails>();

            CreateMap<CreditCardTokenKeyVault, Business.Entities.CreditCardDetails>();
            CreateMap<SharedIntegration.Models.DealDetails, Business.Entities.DealDetails>();

            CreateMap<RefundRequest, CreateTransactionRequest>();
            CreateMap<BlockCreditCardRequest, CreateTransactionRequest>();
            CreateMap<CheckCreditCardRequest, CreateTransactionRequest>();
            CreateMap<InitalBillingDealRequest, CreateTransactionRequest>();
            CreateMap<NextBillingDealRequest, CreateTransactionRequest>();
        }
    }
}
