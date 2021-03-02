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
                // TODO: redo with base date

                //var dateTime = CommonFiltertingExtensions.QuickDateToDateTime(filter.QuickDateFilter.Value);

                //// TODO: use invoice date
                //if (filter.DateType == DateFilterTypeEnum.Created)
                //{
                //    src = src.Where(t => t.InvoiceTimestamp >= dateTime);
                //}
                //else if (filter.DateType == DateFilterTypeEnum.Updated)
                //{
                //    src = src.Where(t => t.UpdatedDate >= dateTime);
                //}
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
