using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Enums = Transactions.Shared.Enums;
using IntegrationModels = Shared.Integration.Models;

namespace Transactions.Api.Models.Transactions
{
    public class TransactionRequest
    {
        public long TerminalID { get; set; }

        /// <summary>
        /// Reference to first installment or to original transaction in case of refund
        /// </summary>
        public long? InitialTransactionID { get; set; }

        [EnumDataType(typeof(Enums.TransactionTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public Enums.TransactionTypeEnum TransactionType { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// 50 telephone deal
        /// 00 regular (megnetic)
        /// </summary>
        [EnumDataType(typeof(Enums.CardPresenceEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public Enums.CardPresenceEnum CardPresence { get; set; }

        /// <summary>
        /// Number Of Installments
        /// </summary>
        [Range(1, 100)]
        [Required(AllowEmptyStrings = false)]
        public int NumberOfPayments { get; set; }

        /// <summary>
        /// Current installment
        /// </summary>
        //public int CurrentInstallment { get; set; }

        /// <summary>
        /// This transaction amount
        /// </summary>
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        [Required(AllowEmptyStrings = false)]
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
        /// Deal information
        /// </summary>
        public IntegrationModels.DealDetails DealDetails { get; set; }
    }
}
