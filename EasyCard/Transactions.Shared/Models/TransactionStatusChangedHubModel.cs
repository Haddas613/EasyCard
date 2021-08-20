using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Transactions.Shared.Enums;

namespace Transactions.Shared.Models
{
    public class TransactionStatusChangedHubModel
    {
        public Guid PaymentTransactionID { get; set; }

        [EnumDataType(typeof(TransactionStatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionStatusEnum Status { get; set; }

        public string StatusString { get; set; }
    }
}
