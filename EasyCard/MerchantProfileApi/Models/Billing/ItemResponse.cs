using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Billing
{
    public class ItemResponse
    {
        public Guid ItemID { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public string ItemName { get; set; }

        public decimal Price { get; set; }

        public CurrencyEnum Currency { get; set; }

        public DateTime? Created { get; set; }

        public string OperationDoneBy { get; set; }

        public string CorrelationId { get; set; }

        public string ExternalReference { get; set; }
    }
}
