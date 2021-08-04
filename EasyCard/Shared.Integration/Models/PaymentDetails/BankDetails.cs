using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models.PaymentDetails
{
    /// <summary>
    /// Not for invoice. See <see cref="BankTransferDetails"></see>
    /// </summary>
    public class BankDetails : PaymentDetails
    {
        public BankDetails()
        {
            PaymentType = PaymentTypeEnum.Bank;
        }

        [JsonProperty(PropertyName = "bank")]
        public int? Bank { get; set; }

        [JsonProperty(PropertyName = "bankBranch")]
        public int? BankBranch { get; set; }

        [JsonProperty(PropertyName = "bankAccount")]
        public string BankAccount { get; set; }
    }
}
