using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Enums = Transactions.Shared.Enums;
using IntegrationModels = Shared.Integration.Models;

namespace Transactions.Api.Models.Transactions
{
    public class TransactionResponse
    {
        public Guid PaymentTransactionID { get; set; }

        /// <summary>
        /// Reference to first installment or to original transaction in case of refund
        /// </summary>
        public long? InitialTransactionID { get; set; }

        /// <summary>
        /// Clearing house merchant reference
        /// </summary>
        public string AggregatorTerminalID { get; set; }

        /// <summary>
        /// Shva terminal ID
        /// </summary>
        public string ProcessorTerminalID { get; set; }

        /// <summary>
        /// Individual counter per terminal
        /// </summary>
        public long TransactionNumber { get; set; }

        public Guid TerminalID { get; set; }

        public Guid MerchantID { get; set; }

        /// <summary>
        /// Shva or other processor
        /// </summary>
        public long? ProcessorID { get; set; }

        /// <summary>
        /// Clearing House or Upay
        /// </summary>
        public long? AggregatorID { get; set; }

        /// <summary>
        /// EasyInvoice or RapidOne
        /// </summary>
        public long? InvoicingID { get; set; }

        /// <summary>
        /// Marketer ID
        /// </summary>
        public long? MarketerID { get; set; }

        public Enums.TransactionStatusEnum Status { get; set; }

        [EnumDataType(typeof(Enums.TransactionTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public Enums.TransactionTypeEnum TransactionType { get; set; }

        /// <summary>
        /// Rejection Reason
        /// </summary>
        public Enums.RejectionReasonEnum? RejectionReason { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// 50 telephone deal
        /// 00 regular (megnetic)
        /// </summary>
        public Enums.CardPresenceEnum CardPresence { get; set; }

        /// <summary>
        /// Number Of Installments
        /// </summary>
        public int NumberOfPayments { get; set; }

        /// <summary>
        /// Current installment
        /// </summary>
        public int CurrentInstallment { get; set; }

        /// <summary>
        /// This transaction amount
        /// </summary>
        public decimal TransactionAmount { get; set; }

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
        /// Legal transaction day
        /// </summary>
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// Date-time when transaction created initially in UTC
        /// </summary>
        public DateTime? TransactionTimestamp { get; set; }

        /// <summary>
        /// Date-time when transaction status updated
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Reference to billing system
        /// </summary>
        public long? BillingOrderID { get; set; }

        // TODO: json converter

        /// <summary>
        /// Concurrency key
        /// </summary>
        public byte[] UpdateTimestamp { get; set; }

        public CreditCardDetails CreditCardDetails { get; set; }

        /// <summary>
        /// Deal information
        /// </summary>
        public IntegrationModels.DealDetails DealDetails { get; set; }

        public JObject ShvaTransactionDetails { get; set; }

        public JObject ClearingHouseTransactionDetails { get; set; }
    }
}
