using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ChequeDetails : BankDetails
    {
        public ChequeDetails()
        {
            PaymentType = Shared.Enums.PaymentTypeEnum.Cheque;
        }

        [JsonProperty(PropertyName = "chequeNumber")]
        public string ChequeNumber { get; set; }

        [JsonProperty(PropertyName = "chequeDate")]
        public DateTime? ChequeDate { get; set; }
    }
}
