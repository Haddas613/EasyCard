using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Reporting.Shared.Models;
using Shared.Business.Security;
using Shared.Helpers.Security;
using SharedIntegration = Shared.Integration;
using Transactions.Shared.Enums;
using Shared.Integration.Models;
using Shared.Helpers;
using Shared.Api.Extensions.Filtering;

namespace Reporting.Business.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly string connectionString;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        public DashboardService(string connectionString, IHttpContextAccessorWrapper httpContextAccessor)
        {
            this.connectionString = connectionString;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<TransactionsTotals>> GetTransactionsTotals(MerchantDashboardQuery request)
        {
            NormalizeFilter(request);

            var query = new
            {
                TerminalID = request.TerminalID,
                MerchantID = httpContextAccessor.GetUser().GetMerchantID(),
                RegularSpecialType = SpecialTransactionTypeEnum.RegularDeal,
                RefundSpecialType = SpecialTransactionTypeEnum.Refund,
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
            };

            var sql = @"select 
    sum(case when t.SpecialTransactionType = @RegularSpecialType then 1 else 0 end) as RegularTransactionsCount,
    sum(case when t.SpecialTransactionType = @RefundSpecialType then 1 else 0 end) as RefundTransactionsCount,
    sum(case when t.SpecialTransactionType = @RegularSpecialType then t.TotalAmount else 0 end) as RegularTransactionsAmount,
    sum(case when t.SpecialTransactionType = @RefundSpecialType then t.TotalAmount else 0 end) as RefundTransactionsAmount
    from [dbo].[PaymentTransaction] as t where
    t.MerchantID = @MerchantID and t.TerminalID = @TerminalID 
    and t.QuickType IN (0, -1)
    and t.TransactionDate <= @DateTo and t.TransactionDate >= @DateFrom";

            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsync<TransactionsTotals>(sql, query);
            }
        }

        public async Task<IEnumerable<PaymentTypeTotals>> GetPaymentTypeTotals(MerchantDashboardQuery request)
        {
            NormalizeFilter(request);

            var query = new
            {
                TerminalID = request.TerminalID,
                MerchantID = httpContextAccessor.GetUser().GetMerchantID(),
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
            };

            var sql = @"select sum(t.TotalAmount) as TotalAmount, t.PaymentTypeEnum from [dbo].[PaymentTransaction] as t where 
    t.MerchantID = @MerchantID and t.TerminalID = @TerminalID 
    and t.QuickType = 0
    and t.TransactionDate <= @DateTo and t.TransactionDate >= @DateFrom
	group by t.PaymentTypeEnum";

            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsync<PaymentTypeTotals>(sql, query);
            }
        }

        public async Task<TransactionTimelines> GetTransactionTimeline(MerchantDashboardQuery request)
        {
            NormalizeFilter(request);

            var response = new TransactionTimelines
            {
                 DateFrom = request.DateFrom,
                 DateTo = request.DateTo,
                 AltDateFrom = request.AltDateFrom,
                 AltDateTo = request.AltDateTo,
                 Granularity = request.Granularity
            };

            var query = new
            {
                TerminalID = request.TerminalID,
                MerchantID = httpContextAccessor.GetUser().GetMerchantID(),
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
            };

            if (!(request.DateFrom.HasValue && request.DateTo.HasValue))
            {
                throw new Exception("Both DateFrom and DateTo should be specified");
            }

            string grouping = request.Granularity.ToString() ?? "Date";
            // TODO: enum from request
            var measure = "TotalAmount";

            var sql = @"select isnull(sum(t.[/**measure**/]),0) as [Measure], d.[Year], d.[/**grouping**/] as [DimensionValue]
	from [dbo].[Timeline] as d left outer join 
	[dbo].[PaymentTransaction] as t on d.[Date] = t.[TransactionDate] and t.MerchantID = @MerchantID and t.TerminalID = @TerminalID and t.QuickType = 0
	where d.[Date] <= @DateTo and  d.[Date] >= @DateFrom
	group by d.[Year], d.[/**grouping**/]
	order by d.[Year], d.[/**grouping**/]";

            sql = sql.Replace("/**measure**/", measure).Replace("/**grouping**/", grouping);
           
            using (var connection = new SqlConnection(connectionString))
            {
                response.GivenPeriod = await connection.QueryAsync<TransactionTimeline>(sql, query);

                response.GivenPeriodMeasure = response.GivenPeriod.Sum(d => d.Measure);

                if (request.AltDateFrom.HasValue || request.AltDateTo.HasValue)
                {
                    var altQuery = new
                    {
                        TerminalID = request.TerminalID,
                        MerchantID = httpContextAccessor.GetUser().GetMerchantID(),
                        DateFrom = request.AltDateFrom,
                        DateTo = request.AltDateTo,
                    };

                    response.AltPeriod = await connection.QueryAsync<TransactionTimeline>(sql, altQuery);

                    response.AltPeriodMeasure = response.AltPeriod.Sum(d => d.Measure);
                }
            }

            return response;
        }

        // TODO: join and filter tran status
        public async Task<IEnumerable<ItemsTotals>> GetItemsTotals(MerchantDashboardQuery request)
        {
            NormalizeFilter(request);

            var query = new
            {
                TerminalID = request.TerminalID,
                MerchantID = httpContextAccessor.GetUser().GetMerchantID(),
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
            };

            var sql = @"select top (5) ROW_NUMBER() OVER(ORDER BY items.TotalAmount DESC) AS RowN, items.[ItemName], items.TotalAmount
	from (
    select t.[ItemName], sum(t.Amount) as TotalAmount from [dbo].[PaymentTransactionItem] as t
	where t.MerchantID = @MerchantID and t.TerminalID = @TerminalID and t.TransactionDate <= @DateTo and t.TransactionDate >= @DateFrom
	group by t.[ItemName]
	) items";

            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsync<ItemsTotals>(sql, query);
            }
        }

        // TODO: use exists
        public async Task<IEnumerable<ConsumersTotals>> GetConsumersTotals(MerchantDashboardQuery request)
        {
            NormalizeFilter(request);

            var query = new
            {
                TerminalID = request.TerminalID,
                MerchantID = httpContextAccessor.GetUser().GetMerchantID(),
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
            };

            var sql = @"select count(*) as CustomersCount, AVG(a.TotalAmount) as AverageAmount, 
	sum(case when b.MinTransactionDate < @DateFrom then a.TotalAmount else 0 end) as RepeatingCustomers,
	sum(case when b.MinTransactionDate >= @DateFrom then a.TotalAmount else 0 end) as NewCustomers,
	sum(a.TotalAmount) as TotalAmount
	from
	(
	select sum(t.TransactionAmount) as TotalAmount, t.ConsumerRef
	from [dbo].[PaymentTransaction] as t 
	where t.MerchantID = @MerchantID and t.TerminalID = @TerminalID and t.TransactionDate <= @DateTo and t.TransactionDate >= @DateFrom
	group by t.ConsumerRef
	) as a
	left outer join 
	(
	select MIN(t.TransactionDate) as MinTransactionDate, t.ConsumerRef
	from [dbo].[PaymentTransaction] as t 
	where t.MerchantID = @MerchantID and t.TerminalID = @TerminalID 
	group by t.ConsumerRef
	) as b on b.ConsumerRef = a.ConsumerRef";

            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsync<ConsumersTotals>(sql, query);
            }
        }

        private void NormalizeFilter(MerchantDashboardQuery request)
        {
            // If not enough info
            if (!request.QuickDateFilter.HasValue || request.DateTo.HasValue || request.DateFrom.HasValue)
            {
                if (request.DateTo == null)
                {
                    request.DateTo = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;
                }

                if (request.DateFrom == null)
                {
                    request.DateFrom = request.DateTo.Value.AddDays(-30).Date;
                }

                if (!request.Granularity.HasValue)
                {
                    request.Granularity = CommonFiltertingExtensions.GetReportGranularity(request.DateFrom.Value, request.DateTo.Value);
                }
            }
            else
            {
                var dateRange = CommonFiltertingExtensions.QuickDateToDateRange(request.QuickDateFilter.Value);
                request.DateFrom = dateRange.DateFrom;
                request.DateTo = dateRange.DateTo;

                if (!request.Granularity.HasValue)
                {
                    request.Granularity = CommonFiltertingExtensions.GetReportGranularity(request.QuickDateFilter.Value);
                }
            }
        }
    }
}
