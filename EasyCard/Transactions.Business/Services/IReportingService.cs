using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Transactions.Business.Entities.Reporting;

namespace Transactions.Business.Services
{
    public interface IReportingService
    {
        IQueryable<BillingSummaryReport> GetBillingSummaryReport(bool sharedTerminal);
    }
}
