using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Shared.Helpers;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
using Shared.Integration.Models.PaymentDetails;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions;
using Transactions.Shared.Models;
using IntegrationModels = Shared.Integration.Models;
using TransactionsApi = Transactions.Api;

namespace Transactions.Api.Models.Invoicing
{
    public class InvoiceRequest
    {
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

        [StringLength(50, MinimumLength = 2)]
        public string CardOwnerName { get; set; }

        [StringLength(20)]
        public string CardOwnerNationalID { get; set; }

        /// <summary>
        /// Invoice amount (should be omitted in case of installment deal)
        /// </summary>
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? InvoiceAmount { get; set; }

        [Range(0, 1)]
        [DataType(DataType.Currency)]
        public decimal VATRate { get; set; }

        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal VATTotal { get; set; }

        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal NetTotal { get; set; }

        /// <summary>
        /// Installment payments details (should be omitted in case of regular deal)
        /// </summary>
        public InstallmentDetails InstallmentDetails { get; set; }

        /// <summary>
        /// Credit card information
        /// </summary>
        [Obsolete]
        public TransactionsApi.Models.Transactions.CreditCardDetails CreditCardDetails { get; set; }

        /// <summary>
        /// Array of payment details, e.g. CreditCardDetails, ChequeDetails etc.
        /// </summary>
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
    }
}
