using AutoMapper;
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
        }
    }
}
