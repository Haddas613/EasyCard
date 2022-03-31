using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using Shared.Helpers.Models.Attributes;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
using Shared.Integration.Models.PaymentDetails;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Shared.Enums;

namespace Transactions.Api.Models.Invoicing
{
    public class InvoiceExcelSummary
    {

        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Date-time when deal created initially in UTC
        /// </summary>
        public DateTime? InvoiceTimestamp { get; set; }

        /// <summary>
        /// Legal invoice day
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        [EnumDataType(typeof(InvoiceTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public InvoiceTypeEnum InvoiceType { get; set; }

        [EnumDataType(typeof(PaymentTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentTypeEnum? PaymentType
        {
            get
            {
                return PaymentDetails?.Select(x => x.PaymentType).FirstOrDefault();
            }
        }

        [ExcelIgnore]
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }

        /// <summary>
        /// Processing status
        /// </summary>
        [EnumDataType(typeof(InvoiceStatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public InvoiceStatusEnum Status { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        public decimal? AmountWithVat { get; set; }

        public decimal? AmountWithoutVat { get; set; }

        public string CardOwnerName { get; set; }
    }
}
