using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.PaymentRequests.Enums;
using Transactions.Shared.Enums;

namespace Transactions.Api.Models.PaymentRequests
{
    public class PaymentRequestSummary
    {
        /// <summary>
        /// Primary reference
        /// </summary>
        public Guid PaymentRequestID { get; set; }

        /// <summary>
        /// Date-time when deal created initially in UTC
        /// </summary>
        public DateTime? PaymentRequestTimestamp { get; set; }

        /// <summary>
        /// Due date
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Terminal
        /// </summary>
        public Guid? TerminalID { get; set; }

        [EnumDataType(typeof(PaymentRequestStatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentRequestStatusEnum Status { get; set; }

        [EnumDataType(typeof(PayReqQuickStatusFilterTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public PayReqQuickStatusFilterTypeEnum QuickStatus { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        public decimal? PaymentRequestAmount { get; set; }

        public string CardOwnerName { get; set; }

        public Guid? ConsumerID { get; set; }

        public Guid? PaymentTransactionID { get; set; }
    }
}
