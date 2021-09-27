using Dapper;
using Reporting.Business.Models;
using Reporting.Shared.Helpers;
using Reporting.Shared.Models;
using Reporting.Shared.Models.Admin;
using Shared.Api.Extensions.Filtering;
using Shared.Api.Models.Enums;
using Shared.Helpers;
using Shared.Helpers.Services;
using Shared.Integration;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Business.Services
{
    public class AdminService : IAdminService
    {
        private readonly string connectionString;
        private readonly IAppInsightReaderService appInsightReaderService;

        public AdminService(string connectionString, IAppInsightReaderService appInsightReaderService)
        {
            this.connectionString = connectionString;
            this.appInsightReaderService = appInsightReaderService;
        }

        public async Task<AdminSmsTimelines> GetSmsTotals(DashboardQuery query)
        {
            query.NormalizeFilter();

            var timestampFilter = KustoAgo(query.DateFrom.Value, query.DateTo.Value);
            var granularity = KustoGranularity(query.Granularity.Value);

            var kustoQuery = @$"
                customEvents 
                | where timestamp > ago({timestampFilter})
                | where (name == ""{Metrics.SmsSent}"" or name == ""{Metrics.SmsError}"")
                | summarize count = count() by bin(timestamp, {granularity}), name
                | order by timestamp asc 
            ";

            var aiRes = await appInsightReaderService.GetData<SMSAppInsightsResult>(kustoQuery);

            var response = new AdminSmsTimelines
            {
                DateFrom = query.DateFrom,
                DateTo = query.DateTo,
                Granularity = query.Granularity
            };

            var all = aiRes.Select(e => new SmsTimeline {
                DimensionValue = e.Timestamp,
                Type = e.Name == Metrics.SmsSent ? SmsTimelineTypeEnum.Success : SmsTimelineTypeEnum.Error,
                Measure = e.Count
            });

            response.Success = all.Where(d => d.Type == SmsTimelineTypeEnum.Success);
            response.Error = all.Where(d => d.Type == SmsTimelineTypeEnum.Error);

            response.SuccessMeasure = response.Success.Sum(d => d.Measure);
            response.ErrorMeasure = response.Error.Sum(d => d.Measure);

            return response;
        }

        public async Task<IEnumerable<TransactionsTotals>> GetTransactionsTotals(DashboardQuery request)
        {
            request.NormalizeFilter();

            var query = new
            {
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
    t.QuickType IN (0, -1)
    and t.TransactionDate <= @DateTo and t.TransactionDate >= @DateFrom";

            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsync<TransactionsTotals>(sql, query);
            }
        }

        public async Task<IEnumerable<MerchantsTotals>> GetMerchantsTotals(DashboardQuery request)
        {
            request.NormalizeFilter();

            var query = new
            {
                RegularSpecialType = SpecialTransactionTypeEnum.RegularDeal,
                RefundSpecialType = SpecialTransactionTypeEnum.Refund,
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
            };

            var sql = @"select top (5) ROW_NUMBER() OVER(ORDER BY transactions.TotalAmount DESC) AS RowN, transactions.[MerchantID], transactions.TotalAmount
	from (
    select t.[MerchantID], sum(case when t.SpecialTransactionType = @RegularSpecialType then t.TotalAmount else 0 end) as TotalAmount 
    from [dbo].[PaymentTransaction] as t where t.TransactionDate <= @DateTo and t.TransactionDate >= @DateFrom and t.QuickType IN (0, -1)
	group by t.[MerchantID]
	) transactions";

            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsync<MerchantsTotals>(sql, query);
            }
        }

        private string KustoAgo(DateTime from, DateTime to)
        {
            return $"{(to - from).TotalDays}d";
        }

        private string KustoGranularity(ReportGranularityEnum reportGranularity)
        {
            return reportGranularity switch
            {
                ReportGranularityEnum.Date => "1d",
                ReportGranularityEnum.Week => "7d",
                ReportGranularityEnum.Month => "30d",
                _ => "7d"
            };
        }
    }
}
