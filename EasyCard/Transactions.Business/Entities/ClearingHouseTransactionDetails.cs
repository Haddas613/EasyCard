using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class ClearingHouseTransactionDetails
    {
        public long? ClearingHouseTransactionID { get; set; }

        public Guid? MerchantReference { get; set; }

        //Do not store this field to db
        [JsonIgnore]
        public string ConcurrencyToken { get; set; }
    }
}
