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
            if (!string.IsNullOrWhiteSpace(filter.Email))
            {
                src = src.Where(t => EF.Functions.Like(t.Email, filter.Email.UseWildCard(true)));
            }

            return src;
        }
    }
}
