using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.UI;
using Shared.Helpers;
using Shared.Helpers.Models.Attributes;
using Shared.Integration.Models.PaymentDetails;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Shared.Enums;
using Transactions.Shared.Enums.Resources;

namespace Transactions.Api.Models.Invoicing
{
    public class InvoiceExcelSummaryAdmin : InvoiceSummaryAmounts
    {
        /// <summary>
        /// Primary reference
        /// </summary>
        public Guid InvoiceID { get; set; }

        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Legal invoice day
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// Terminal
        /// </summary>
        public Guid? TerminalID { get; set; }

        public string InvoiceType { get; set; }

        public string Status { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        public string CardOwnerName { get; set; }

        public Guid? ConsumerID { get; set; }

        public Guid? PaymentTransactionID { get; set; }

        public DocumentOriginEnum DocumentOrigin { get; set; }

        public string MerchantName { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid MerchantID { get; set; }

        public string TerminalName { get; set; }

        [MetadataOptions(Hidden = true)]
        public new DateTime? InvoiceTimestamp { get; set; }
        public string PaymentType
        {
            get
            {
                return PaymentTypeResource.ResourceManager.GetString(PaymentDetails == null ? string.Empty : PaymentDetails?.Select(x => x.PaymentType).FirstOrDefault().ToString(), new CultureInfo("he"));
            }
        }

        [ExcelIgnore]
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }

    }
}
