using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Transactions.Shared.Enums;

namespace Transactions.Api.Models.Transactions
{
    public class TransactionSummary
    {
        public long PaymentTransactionID { get; set; }

        public long TransactionNumber { get; set; }

        public long TerminalID { get; set; }

        public long MerchantID { get; set; }

        public decimal Amount { get; set; }

        [EnumDataType(typeof(TransactionTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionTypeEnum TransactionType { get; set; }

        public string Currency { get; set; }

        public string Code { get; set; }

        public DateTime Created { get; set; }
    }
}
