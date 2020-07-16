using MerchantProfileApi.Models.Billing;
using Merchants.Business.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Extensions
{
    public static class ConsumerFilterExtensions
    {
        public static IQueryable<Consumer> Filter(this IQueryable<Consumer> src, ConsumersFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search) && filter.Search.Trim().Length > 3)
            {
                var search = filter.Search.Trim();
                src = src.Where(c => EF.Functions.Like(c.ConsumerName, $"%{search}%")
                || EF.Functions.Like(c.ConsumerEmail, $"%{search}%")
                || EF.Functions.Like(c.ConsumerPhone, $"%{search}%"));
            }

            return src;
        }
    }
}
