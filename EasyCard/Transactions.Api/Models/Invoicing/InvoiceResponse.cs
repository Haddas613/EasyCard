using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IntegrationModels = Shared.Integration.Models;

namespace Transactions.Api.Models.Invoicing
{
    public class InvoiceResponse
    {
        /// <summary>
        /// Primary reference
        /// </summary>
        public Guid InvoiceID { get; set; }

        /// <summary>
        /// Date-time when deal created initially in UTC
        /// </summary>
        public DateTime? InvoiceTimestamp { get; set; }

        /// <summary>
        /// Legal invoice day
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// Invoice details
        /// </summary>
        public InvoiceDetails InvoiceDetails { get; set; }

        /// <summary>
        /// EasyCard terminal reference
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public Guid TerminalID { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Deal information (optional)
        /// </summary>
        public IntegrationModels.DealDetails DealDetails { get; set; }

        public string CardOwnerName { get; set; }

        public string CardOwnerNationalID { get; set; }

        /// <summary>
        /// Invoice amount (should be omitted in case of installment deal)
        /// </summary>
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? InvoiceAmount { get; set; }

        /// <summary>
        /// Tax rate (VAT)
        /// </summary>
        [Range(0.01, 1)]
        [DataType(DataType.Currency)]
        public decimal? TaxRate { get; set; }

        /// <summary>
        /// Tax amount
        /// </summary>
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? TaxAmount { get; set; }

        /// <summary>
        /// Installment payments details (should be omitted in case of regular deal)
        /// </summary>
        public InstallmentDetails InstallmentDetails { get; set; }
    }
}
