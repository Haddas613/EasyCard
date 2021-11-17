using AutoMapper;
using MerchantProfileApi.Models.Billing;
using Merchants.Business.Entities.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Mapping
{
    public class ItemProfile : Profile
    {
        public ItemProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterItemMappings();
        }

        private void RegisterItemMappings()
        {
            CreateMap<Item, ItemSummary>();

            CreateMap<Item, ItemResponse>();

            CreateMap<ItemRequest, Item>()
                .ForMember(d => d.Active, o => o.MapFrom(d => d.Active.GetValueOrDefault(true)));

            CreateMap<UpdateItemRequest, Item>()
                .ForMember(d => d.ItemID, o => o.Ignore())
                .ForMember(d => d.UpdateTimestamp, o => o.Ignore());

            // NOTE: this is security assignment
            CreateMap<Merchants.Business.Entities.Terminal.Terminal, Item>()
                .ForMember(d => d.MerchantID, o => o.MapFrom(d => d.MerchantID))
                .ForAllOtherMembers(d => d.Ignore());
        }
    }
}
