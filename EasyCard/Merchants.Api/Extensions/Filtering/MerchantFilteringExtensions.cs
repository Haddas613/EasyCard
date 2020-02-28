using Merchants.Api.Models.Merchant;
using Merchants.Business.Entities.Merchant;
using Microsoft.EntityFrameworkCore;
using Shared.Helpers;
using System.Linq;

namespace Merchants.Api.Extensions.Filtering
{
    public static class MerchantFilteringExtensions
    {
        public static IQueryable<Merchant> Filter(this IQueryable<Merchant> src, MerchantsFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.BusinessName))
            {
                src = src.Where(t => EF.Functions.Like(t.BusinessName, filter.BusinessName.UseWildCard(true)));
            }

            return src;
        }
    }
}
