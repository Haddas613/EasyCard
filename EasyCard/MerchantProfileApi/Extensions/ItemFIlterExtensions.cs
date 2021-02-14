using MerchantProfileApi.Models.Billing;
using Merchants.Business.Entities.Billing;
using Microsoft.EntityFrameworkCore;
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
            if (filter.ShowDeleted)
            {
                src = src.Where(d => d.Active == false);
            }
            else
            {
                src = src.Where(d => d.Active == true);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search) && filter.Search.Trim().Length > 3)
            {
                var search = filter.Search.Trim();
                src = src.Where(c => EF.Functions.Like(c.ItemName, $"%{search}%"));
            }

            return src;
        }
    }
}
