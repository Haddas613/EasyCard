using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using Shared.Helpers.Models.Attributes;
using Shared.Integration.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Transactions.Api.Models.Billing
{
    public class BillingDealExcelSummary
    {
        public string TerminalName { get; set; }

        public string MerchantName { get; set; }

        [ExcelIgnore]
        public Guid TerminalID { get; set; }

        [ExcelIgnore]
        public Guid MerchantID { get; set; }

        public decimal TransactionAmount { get; set; }

        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        public DateTime? BillingDealTimestamp { get; set; }

        public DateTime? NextScheduledTransaction { get; set; }

        public string ConsumerName { get; set; }

        public string CardNumber { get; set; }

        public bool? CardExpired { get; set; }

        public bool Active { get; set; }

        public int? CurrentDeal { get; set; }

        public DateTime? PausedFrom { get; set; }

        public DateTime? PausedTo { get; set; }

        public bool Paused { get; set; }

        public PaymentTypeEnum PaymentType { get; set; }

        public bool InvoiceOnly { get; set; }
    }
}
