using MerchantProfileApi.Models.Billing;
using Merchants.Business.Entities.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Extensions
{
    public static class ConsumerFIlterExtensions
    {
        public static IQueryable<Consumer> Filter(this IQueryable<Consumer> src, ConsumersFilter filter)
        {
            return src;
        }
    }
}
