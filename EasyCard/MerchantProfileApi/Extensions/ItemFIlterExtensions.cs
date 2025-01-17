﻿using MerchantProfileApi.Models.Billing;
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
            if (filter.ShowDeleted == Shared.Helpers.Models.ShowDeletedEnum.OnlyDeleted)
            {
                src = src.Where(d => d.Active == false);
            }
            else if (filter.ShowDeleted == Shared.Helpers.Models.ShowDeletedEnum.OnlyActive)
            {
                src = src.Where(d => d.Active == true);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search) && filter.Search.Trim().Length > 3)
            {
                var search = filter.Search.Trim();
                src = src.Where(c => EF.Functions.Like(c.ItemName, $"%{search}%"));
            }

            if (!string.IsNullOrWhiteSpace(filter.ExternalReference))
            {
                src = src.Where(d => d.ExternalReference == filter.ExternalReference);
            }

            if (!string.IsNullOrWhiteSpace(filter.BillingDesktopRefNumber))
            {
                src = src.Where(d => d.BillingDesktopRefNumber == filter.BillingDesktopRefNumber);
            }

            if (!string.IsNullOrWhiteSpace(filter.Origin))
            {
                src = src.Where(c => c.Origin == filter.Origin);
            }

            if (!string.IsNullOrWhiteSpace(filter.EcwidID))
            {
                src = src.Where(d => d.EcwidID == filter.EcwidID);
            }

            if (!string.IsNullOrWhiteSpace(filter.WoocommerceID))
            {
                src = src.Where(d => d.WoocommerceID == filter.WoocommerceID);
            }

            return src;
        }
    }
}
