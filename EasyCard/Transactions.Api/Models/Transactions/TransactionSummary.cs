using Newtonsoft.Json.Converters;
using Shared.Helpers;
using Shared.Integration.Models;
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
        public Guid PaymentTransactionID { get; set; }

        public Guid TerminalID { get; set; }

        public Guid MerchantID { get; set; }

        public decimal TransactionAmount { get; set; }

        [EnumDataType(typeof(TransactionTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionTypeEnum TransactionType { get; set; }

        public CurrencyEnum Currency { get; set; }

        public DateTime Created { get; set; }
    }
}
