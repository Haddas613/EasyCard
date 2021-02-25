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

        public async Task<MerchantDashboardResponse> GetMerchantDashboard(MerchantDashboardQuery query)
        {
            query.MerchantID = httpContextAccessor.GetUser().GetMerchantID();

            using (var connection = new SqlConnection(connectionString))
            {
                var response = new MerchantDashboardResponse
                {
                   
                };

                using (var multi = await connection.QueryMultipleAsync("PR_MerchantsDashboard", query, commandType: System.Data.CommandType.StoredProcedure))
                {
                    response.TransactionsTotals = await multi.ReadAsync<TransactionsTotals>();

                    response.PaymentTypeTotal = await multi.ReadAsync<PaymentTypeTotals>();

                    response.TransactionTimeline = await multi.ReadAsync<TransactionTimeline>();

                    response.ItemsTotals = await multi.ReadAsync<ItemsTotals>();

                    response.ConsumersTotals = await multi.ReadAsync<ConsumersTotals>();
                }

                return response;
            }
        }

        public async Task<IEnumerable<TransactionsTotals>> GetTransactionsTotals(MerchantDashboardQuery query)
        {
            query.MerchantID = httpContextAccessor.GetUser().GetMerchantID();

            var sql = @"select count(*) as TransactionsCount, sum(t.TransactionAmount) as TotalAmount from [dbo].[PaymentTransaction] as t 
	where t.MerchantID = @MerchantID and t.TerminalID = @TerminalID and t.TransactionDate <= @DateTo and t.TransactionDate >= @DateFrom";

            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsync<TransactionsTotals>(sql, query);
            }
        }

        public async Task<IEnumerable<PaymentTypeTotals>> GetPaymentTypeTotals(MerchantDashboardQuery query)
        {
            query.MerchantID = httpContextAccessor.GetUser().GetMerchantID();

            var sql = @"select sum(t.TransactionAmount) as TotalAmount, t.PaymentTypeEnum from [dbo].[PaymentTransaction] as t
	where t.MerchantID = @MerchantID and t.TerminalID = @TerminalID and t.TransactionDate <= @DateTo and t.TransactionDate >= @DateFrom
	group by t.PaymentTypeEnum";

            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsync<PaymentTypeTotals>(sql, query);
            }
        }

        public async Task<IEnumerable<TransactionTimeline>> GetTransactionTimeline(MerchantDashboardQuery query)
        {
            query.MerchantID = httpContextAccessor.GetUser().GetMerchantID();

            var sql = @"select TOP(100) isnull(sum(t.TransactionAmount),0) as TotalAmount, d.[Date]
	from [dbo].[Timeline] as d left outer join 
	[dbo].[PaymentTransaction] as t on d.[Date] = t.[TransactionDate]
	where t.MerchantID = @MerchantID and t.TerminalID = @TerminalID and d.[Date] <= @TimelineDateTo and  d.[Date] >= @TimelineDateFrom
	group by d.[Date]
	order by d.[Date]";

            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsync<TransactionTimeline>(sql, query);
            }
        }

        public async Task<IEnumerable<ItemsTotals>> GetItemsTotals(MerchantDashboardQuery query)
        {
            query.MerchantID = httpContextAccessor.GetUser().GetMerchantID();

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

        public async Task<IEnumerable<ConsumersTotals>> GetConsumersTotals(MerchantDashboardQuery query)
        {
            query.MerchantID = httpContextAccessor.GetUser().GetMerchantID();

            var sql = @"select count(*) as CustomersCount, AVG(a.TotalAmount) as AverageAmount, 
	sum(case when b.MinTransactionDate < @TimelineDateFrom then a.TotalAmount else 0 end) as RepeatingCustomers,
	sum(case when b.MinTransactionDate >= @TimelineDateFrom then a.TotalAmount else 0 end) as NewCustomers,
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
    }
}
