using Merchants.Api.Models.Merchant;
using Merchants.Api.Models.User;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.User;
using Microsoft.EntityFrameworkCore;
using Shared.Helpers;
using System.Linq;

namespace Merchants.Api.Extensions.Filtering
{
    public static class UserFilteringExtensions
    {
        public static IQueryable<UserTerminalMapping> Filter(this IQueryable<UserTerminalMapping> src, GetUsersFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search) && filter.Search.Length > 2)
            {
                src = src.Where(t => EF.Functions.Like(t.Email, filter.Search.UseWildCard(true)) || EF.Functions.Like(t.DisplayName, filter.Search.UseWildCard(true)));
            }

            if (filter.SearchGuid.HasValue)
            {
                src = src.Where(t => t.UserID == filter.SearchGuid.Value || t.MerchantID == filter.SearchGuid.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Email) && filter.Email.Length > 2)
            {
                src = src.Where(t => EF.Functions.Like(t.Email, filter.Email.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Name) && filter.Name.Length > 2)
            {
                src = src.Where(t => EF.Functions.Like(t.DisplayName, filter.Name.UseWildCard(true)));
            }

            if (filter.Status.HasValue)
            {
                src = src.Where(t => t.Status == filter.Status.Value);
            }

            return src;
        }
    }
}
