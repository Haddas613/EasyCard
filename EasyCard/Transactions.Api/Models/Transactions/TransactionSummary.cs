using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.UI;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions.Enums;
using Transactions.Shared.Enums;

namespace Transactions.Api.Models.Transactions
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TransactionSummary
    {
        [MetadataOptions(Order = 1)]
        public Guid PaymentTransactionID { get; set; }

        [MetadataOptions(Order = 2)]
        public Guid TerminalID { get; set; }

        public decimal TransactionAmount { get; set; }

        [EnumDataType(typeof(TransactionTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionTypeEnum TransactionType { get; set; }

        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        public DateTime? TransactionTimestamp { get; set; }

        /// <summary>
        /// Processing status
        /// </summary>
        [EnumDataType(typeof(TransactionStatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionStatusEnum Status { get; set; }

        /// <summary>
        /// Payment Type
        /// </summary>
        [EnumDataType(typeof(PaymentTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentTypeEnum PaymentTypeEnum { get; set; }

        [EnumDataType(typeof(QuickStatusFilterTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public QuickStatusFilterTypeEnum QuickStatus { get; set; }

        /// <summary>
        /// Special transaction type
        /// </summary>
        [EnumDataType(typeof(SpecialTransactionTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
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
        public RejectionReasonEnum? RejectionReason { get; set; }

        /// <summary>
        /// Telephone deal or Regular (megnetic)
        /// </summary>
        [EnumDataType(typeof(CardPresenceEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CardPresenceEnum CardPresence { get; set; }

        public string CardOwnerName { get; set; }

        [MetadataOptions(Order = 3)]
        public string ShvaDealID { get; set; }
    }
}
