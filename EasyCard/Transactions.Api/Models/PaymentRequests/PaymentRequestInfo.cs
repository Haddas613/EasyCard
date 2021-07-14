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
using IntegrationModels = Shared.Integration.Models;

namespace Transactions.Api.Models.PaymentRequests
{
    public class PaymentRequestInfo
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
        /// Currency
        /// </summary>
        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        [EnumDataType(typeof(PaymentRequestStatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentRequestStatusEnum Status { get; set; }

        [EnumDataType(typeof(PayReqQuickStatusFilterTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public PayReqQuickStatusFilterTypeEnum QuickStatus { get; set; }

        public string CardOwnerName { get; set; }

        public string CardOwnerNationalID { get; set; }

        /// <summary>
        /// Deal information (optional)
        /// </summary>
        public IntegrationModels.DealDetails DealDetails { get; set; }

        public decimal VATRate { get; set; }

        public decimal VATTotal { get; set; }

        public decimal NetTotal { get; set; }

        /// <summary>
        /// Installment payments details (should be omitted in case of regular deal)
        /// </summary>
        //public IntegrationModels.InstallmentDetails InstallmentDetails { get; set; }

        /// <summary>
        /// Number Of payments (cannot be more than 999)
        /// </summary>
        public int NumberOfPayments { get; set; }

        /// <summary>
        /// Initial installment payment
        /// </summary>
        public decimal InitialPaymentAmount { get; set; }

        /// <summary>
        /// TotalAmount = InitialPaymentAmount + (NumberOfInstallments - 1) * InstallmentPaymentAmount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Amount of one instalment payment
        /// </summary>
        public decimal InstallmentPaymentAmount { get; set; }

        /// <summary>
        /// This amount
        /// </summary>
        public decimal PaymentRequestAmount { get; set; }

        public bool IsRefund { get; set; }
    }
}
