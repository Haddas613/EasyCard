using Microsoft.EntityFrameworkCore;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Transactions.Business.Data;
using Transactions.Business.Entities;
using Transactions.Business.Entities.Reporting;

namespace Transactions.Business.Services
{
    public class ReportingService : IReportingService
    {
        private readonly TransactionsContext context;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ClaimsPrincipal user;

        public ReportingService(TransactionsContext context, IHttpContextAccessorWrapper httpContextAccessor)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        public IQueryable<BillingSummaryReport> GetBillingSummaryReport(bool sharedTerminal)
        {
            return GetBillingDeals(sharedTerminal)
                .Where(d => d.DealDetails.ConsumerExternalReference != null)
                .GroupBy(d => d.DealDetails.ConsumerExternalReference, d => d.TransactionAmount, (d1, d2) =>
                new BillingSummaryReport { ExternalReference = d1, BillingDealsNumber = d2.Count(), TotalTransactionsAmounts = d2.Sum() });
        }

        public IQueryable<BillingDeal> GetBillingDeals(bool sharedTerminal)
        {
            var tokens = context.BillingDeals.AsNoTracking();

            tokens = tokens.Where(t => t.Active);

            if (user.IsAdmin())
            {
                throw new ApplicationException("This method should not be used by Admin");
            }
            else if (user.IsTerminal() && !sharedTerminal)
            {
                var terminalID = user.GetTerminalID()?.FirstOrDefault();
                return tokens.Where(t => t.TerminalID == terminalID);
            }
            else
            {
                var response = tokens.Where(t => t.MerchantID == user.GetMerchantID());
                var terminals = user.GetTerminalID()?.Cast<Guid?>();
                if (terminals?.Count() > 0)
                {
                    response = response.Where(d => terminals.Contains(d.TerminalID));
                }

                return response;
            }
        }
    }
}
