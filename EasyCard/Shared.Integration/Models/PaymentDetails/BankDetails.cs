using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models.PaymentDetails
{
    /// <summary>
    /// Represents abstract payment related to the bank, e.g. cheque or bank transfer
    /// </summary>
    public abstract class BankDetails : PaymentDetails
    {
        [JsonProperty(PropertyName = "bank")]
        public int? Bank { get; set; }

        [JsonProperty(PropertyName = "bankBranch")]
        public int? BankBranch { get; set; }

        [JsonProperty(PropertyName = "bankAccount")]
        public string BankAccount { get; set; }
    }
}
