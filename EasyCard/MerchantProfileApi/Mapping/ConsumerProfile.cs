using AutoMapper;
using MerchantProfileApi.Models.Billing;
using Merchants.Business.Entities.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Mapping
{
    public class ConsumerProfile : Profile
    {
        public ConsumerProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterConsumerMappings();
        }

        private void RegisterConsumerMappings()
        {
            CreateMap<Consumer, ConsumerSummary>()
                .ForMember(s => s.HasBankAccount, src => src.MapFrom(c => c.BankDetails != null));

            CreateMap<Consumer, ConsumerResponse>();

            CreateMap<ConsumerRequest, Consumer>()
                .ForMember(d => d.ConsumerNote, src => src.MapFrom(x => x.Note))
                .ForMember(d => d.ConsumerSecondPhone, src => src.MapFrom(x => x.ConsumerSecondPhone))
                .ForMember(d => d.Active, o => o.MapFrom(d => d.Active.GetValueOrDefault(true)));

            CreateMap<UpdateConsumerRequest, Consumer>()
                .ForMember(d => d.ConsumerID, o => o.Ignore())
                .ForMember(d => d.UpdateTimestamp, o => o.Ignore());
        }
    }
}
