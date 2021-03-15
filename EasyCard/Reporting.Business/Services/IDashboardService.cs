using Reporting.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Business.Services
{
    public interface IDashboardService
    {
        public Task<IEnumerable<TransactionsTotals>> GetTransactionsTotals(MerchantDashboardQuery query);


        public Task<IEnumerable<PaymentTypeTotals>> GetPaymentTypeTotals(MerchantDashboardQuery query);


        public Task<TransactionTimelines> GetTransactionTimeline(MerchantDashboardQuery query);


        public Task<IEnumerable<ItemsTotals>> GetItemsTotals(MerchantDashboardQuery query);


        public Task<IEnumerable<ConsumersTotals>> GetConsumersTotals(MerchantDashboardQuery query);
    }
}
