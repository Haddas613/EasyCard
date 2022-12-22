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
            if (filter.MerchantID.HasValue)
            {
                src = src.Where(t => t.MerchantID == filter.MerchantID.Value);
            }
            else if (filter.MerchantIDs?.Count() > 0)
            {
                src = src.Where(t => filter.MerchantIDs.Contains(t.MerchantID));
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                src = src.Where(t => EF.Functions.Like(t.BusinessName, filter.Search.UseWildCard(true))
                    || EF.Functions.Like(t.MarketingName, filter.Search.UseWildCard(true))
                    || EF.Functions.Like(t.ContactPerson, filter.Search.UseWildCard(true))
                    || EF.Functions.Like(t.BusinessID, filter.Search.UseWildCard(true))
                    || EF.Functions.Like(t.PhoneNumber, filter.Search.UseWildCard(true)));
            }

            return src;
        }
    }
}
