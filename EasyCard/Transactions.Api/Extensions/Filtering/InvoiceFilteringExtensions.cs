using Microsoft.EntityFrameworkCore;
using Shared.Api.Extensions.Filtering;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Invoicing;
using Transactions.Api.Models.Transactions.Enums;
using Transactions.Business.Entities;

namespace Transactions.Api.Extensions.Filtering
{
    public static class InvoiceFilteringExtensions
    {
        public static IQueryable<Invoice> Filter(this IQueryable<Invoice> src, InvoicesFilter filter)
        {
            if (filter.InvoiceID != null)
            {
                src = src.Where(t => t.InvoiceID == filter.InvoiceID);
                return src;
            }

            if (filter.BillingDealID != null)
            {
                src = src.Where(t => t.BillingDealID == filter.BillingDealID);
            }

            if (filter.TerminalID != null)
            {
                src = src.Where(t => t.TerminalID == filter.TerminalID);
            }

            if (filter.Currency != null)
            {
                src = src.Where(t => t.Currency == filter.Currency);
            }

            //TODO: Quick time filters using SequentialGuid https://stackoverflow.com/questions/54920200/entity-framework-core-guid-greater-than-for-paging
            if (filter.QuickDateFilter != null)
            {
                var dateRange = CommonFiltertingExtensions.QuickDateToDateRange(filter.QuickDateFilter.Value);

                src = src.Where(t => t.InvoiceDate >= dateRange.DateFrom && t.InvoiceDate <= dateRange.DateTo);
            }
            else
            {
                if (filter.DateFrom != null)
                {
                    src = src.Where(t => t.InvoiceDate >= filter.DateFrom.Value);
                }

                if (filter.DateTo != null)
                {
                    src = src.Where(t => t.InvoiceDate <= filter.DateTo.Value);
                }
            }

            if (filter.Status != null)
            {
                src = src.Where(t => t.Status == filter.Status.Value);
            }

            if (filter.InvoiceType != null)
            {
                src = src.Where(t => t.InvoiceDetails.InvoiceType == filter.InvoiceType);
            }

            if (filter.InvoiceNumber != null)
            {
                src = src.Where(t => t.InvoiceNumber == filter.InvoiceNumber);
            }

            if (filter.ConsumerID != null)
            {
                src = src.Where(t => t.DealDetails.ConsumerID == filter.ConsumerID);
            }

            if (filter.ConsumerExternalReference != null)
            {
                src = src.Where(t => t.DealDetails.ConsumerExternalReference == filter.ConsumerExternalReference);
            }

            if (!string.IsNullOrWhiteSpace(filter.ConsumerEmail))
            {
                src = src.Where(t => EF.Functions.Like(t.DealDetails.ConsumerEmail, filter.ConsumerEmail.UseWildCard(true)));
            }

            if (filter.InvoiceAmount > 0)
            {
                src = src.Where(t => t.InvoiceAmount >= filter.InvoiceAmount);
            }

            return src;
        }
    }
}
