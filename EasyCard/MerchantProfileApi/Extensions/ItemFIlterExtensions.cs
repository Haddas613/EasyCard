using MerchantProfileApi.Models.Billing;
using Merchants.Business.Entities.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Extensions
{
    public static class ItemFIlterExtensions
    {
        public static IQueryable<Item> Filter(this IQueryable<Item> src, ItemsFilter filter)
        {
            return src;
        }
    }
}
