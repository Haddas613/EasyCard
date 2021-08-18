using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models.PaymentDetails
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ChequeDetails : BankDetails
    {
        public ChequeDetails()
        {
            PaymentType = PaymentTypeEnum.Cheque;
        }

        [JsonProperty(PropertyName = "chequeNumber")]
        public string ChequeNumber { get; set; }

        [JsonProperty(PropertyName = "chequeDate")]
        public DateTime? ChequeDate { get; set; }
    }
}
