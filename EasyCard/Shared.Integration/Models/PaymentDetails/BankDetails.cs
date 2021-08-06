using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Required]
        public int? Bank { get; set; }

        [JsonProperty(PropertyName = "bankBranch")]
        [Required]
        public int? BankBranch { get; set; }

        [JsonProperty(PropertyName = "bankAccount")]
        [Required(AllowEmptyStrings = false)]
        [StringLength(12, MinimumLength = 6)]
        public string BankAccount { get; set; }
    }
}
