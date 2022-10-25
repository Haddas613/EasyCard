using Newtonsoft.Json;

namespace Transactions.Api.Models.Transactions
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TransactionSummaryDetails
    {
        public decimal TransactionAmount { get; set; }

        public decimal InitialPaymentAmount { get; set; }

        public decimal InstallmentPaymentAmount { get; set; }

        public string DealDescription { get; set; }
    }
}
