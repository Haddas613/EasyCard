using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.UI;
using Shared.Helpers;
using Shared.Helpers.Models.Attributes;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Shared.Enums;

namespace Transactions.Api.Models.Transactions
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TransactionSummary
    {
        [MetadataOptions(Order = 1001)]
        public Guid PaymentTransactionID { get; set; }

        public string TerminalName { get; set; }

        [ExcelIgnore]
        [MetadataOptions(Hidden = true)]
        public Guid TerminalID { get; set; }

        [EnumDataType(typeof(DocumentOriginEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public DocumentOriginEnum DocumentOrigin { get; set; }

        public string CardNumber { get; set; }

        /// <summary>
        /// Rejection Reason Message (in case of rejected transaction)
        /// </summary>
        public string RejectionMessage { get; set; }

        public int? ProcessorResultCode { get; set; }

        public decimal TransactionAmount { get; set; }

        public decimal InitialPaymentAmount { get; set; }
        public decimal InstallmentPaymentAmount { get; set; }

        [EnumDataType(typeof(TransactionTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionTypeEnum TransactionType { get; set; }

        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        [MetadataOptions(Hidden = true)]
        public CurrencyEnum Currency { get; set; }

        public DateTime? TransactionTimestamp { get; set; }

        /// <summary>
        /// Processing status
        /// </summary>
        [EnumDataType(typeof(TransactionStatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        [MetadataOptions(Hidden = true)]
        public TransactionStatusEnum Status { get; set; }

        /// <summary>
        /// Payment Type
        /// </summary>
        [EnumDataType(typeof(PaymentTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        [MetadataOptions(Hidden = true)]
        public PaymentTypeEnum PaymentTypeEnum { get; set; }

        [EnumDataType(typeof(QuickStatusFilterTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public QuickStatusFilterTypeEnum QuickStatus { get; set; }

        /// <summary>
        /// Special transaction type
        /// </summary>
        [EnumDataType(typeof(SpecialTransactionTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        [MetadataOptions(Hidden = true)]
        public SpecialTransactionTypeEnum SpecialTransactionType { get; set; }

        /// <summary>
        /// J3, J4, J5
        /// </summary>
        [EnumDataType(typeof(JDealTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public JDealTypeEnum JDealType { get; set; }

        /// <summary>
        /// Rejection Reason
        /// </summary>
        [EnumDataType(typeof(RejectionReasonEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        [MetadataOptions(Hidden = true)]
        public RejectionReasonEnum? RejectionReason { get; set; }

        /// <summary>
        /// Telephone deal or Regular (megnetic)
        /// </summary>
        [EnumDataType(typeof(CardPresenceEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        [MetadataOptions(Hidden = true)]
        public CardPresenceEnum CardPresence { get; set; }

        public string CardOwnerName { get; set; }

        public string ConsumerExternalReference { get; set; }

        [MetadataOptions(Order = 3, Hidden = true)]
        public string ShvaDealID { get; set; }

        [ExcelIgnore]
        [MetadataOptions(Hidden = true)]
        public Guid? InvoiceID { get; set; }

        public string DealDescription { get; set; }
    }
}
