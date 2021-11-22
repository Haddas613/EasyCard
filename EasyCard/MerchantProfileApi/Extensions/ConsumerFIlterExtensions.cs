using MerchantProfileApi.Models.Billing;
using Merchants.Business.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Shared.Helpers;
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
                src = src.Where(c => EF.Functions.Like(c.ConsumerName, $"%{search}%")
                || EF.Functions.Like(c.ConsumerEmail, $"%{search}%")
                || EF.Functions.Like(c.ConsumerNationalID, $"%{search}%")
                || EF.Functions.Like(c.ConsumerPhone, $"%{search}%")
                || EF.Functions.Like(c.ExternalReference, $"%{search}%"));
            }

            if (filter.ConsumerID.HasValue)
            {
                src = src.Where(c => c.ConsumerID == filter.ConsumerID.Value);
            }
            else if (!string.IsNullOrWhiteSpace(filter.ConsumersID))
            {
                List<Guid> ids = new List<Guid>();

                foreach (var str in filter.ConsumersID.Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    if (Guid.TryParse(str, out var guid))
                    {
                        ids.Add(guid);
                    }
                }

                if (ids.Count == 1)
                {
                    src = src.Where(c => c.ConsumerID == ids[0]);
                }
                else if (ids.Count > 1)
                {
                    src = src.Where(c => ids.Contains(c.ConsumerID));
                }
            }

            if (filter.TerminalID.HasValue)
            {
                src = src.Where(c => c.TerminalID == filter.TerminalID.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Email))
            {
                src = src.Where(c => EF.Functions.Like(c.ConsumerEmail, filter.Email.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Phone))
            {
                src = src.Where(c => EF.Functions.Like(c.ConsumerPhone, filter.Phone.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.NationalID))
            {
                src = src.Where(c => c.ConsumerNationalID == filter.NationalID);
            }

            if (!string.IsNullOrWhiteSpace(filter.ExternalReference))
            {
                src = src.Where(c => c.ExternalReference == filter.ExternalReference);
            }

            if (!string.IsNullOrWhiteSpace(filter.BillingDesktopRefNumber))
            {
                src = src.Where(c => c.BillingDesktopRefNumber == filter.BillingDesktopRefNumber);
            }

            return src;
        }
    }
}
