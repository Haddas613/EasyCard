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
    public class TransactionSummaryAdmin : TransactionSummary
    {
        public string MerchantName { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid MerchantID { get; set; }

        public string TerminalName { get; set; }

        [MetadataOptions(Hidden = true)]
        public new Guid TerminalID { get; set; }

        [MetadataOptions(Hidden = true)]
        public new CurrencyEnum Currency { get; set; }

        [EnumDataType(typeof(DocumentOriginEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public DocumentOriginEnum DocumentOrigin { get; set; }

        /// <summary>
        /// Processing status
        /// </summary>
        [EnumDataType(typeof(TransactionStatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        [MetadataOptions(Hidden = true)]
        public new TransactionStatusEnum Status { get; set; }

        /// <summary>
        /// Rejection Reason
        /// </summary>
        [EnumDataType(typeof(RejectionReasonEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        [MetadataOptions(Hidden = true)]
        public new RejectionReasonEnum? RejectionReason { get; set; }

        /// <summary>
        /// Rejection Reason
        /// </summary>
        [MetadataOptions(Hidden = true)]
        public new PaymentTypeEnum PaymentTypeEnum { get; set; }

        public string CardNumber { get; set; }
    }
}
