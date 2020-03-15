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
    public class IntegrationsProfile : Profile
    {
        public IntegrationsProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterTransactionMappings();
        }

        private void RegisterTransactionMappings()
        {
            CreateMap<PaymentTransaction, AggregatorCreateTransactionRequest>(); // TODO: members

            CreateMap<PaymentTransaction, ProcessorTransactionRequest>(); // TODO: members
        }
    }
}
