using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Swagger;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Transactions.Api.Models.Transactions.Enums;
using Transactions.Shared.Enums;
using TransactionTypeEnum = Shared.Integration.Models.TransactionTypeEnum;

namespace Transactions.Api.Models.Transactions
{
    public class TransactionsFilter : FilterBase
    {
        public Guid? TerminalID { get; set; }

        [SwaggerExclude]
        public Guid? MerchantID { get; set; }

        public Guid? PaymentTransactionID { get; set; }

        [Range(0, double.PositiveInfinity)]
        public decimal? AmountFrom { get; set; }

        [Range(0, double.PositiveInfinity)]
        public decimal? AmountTo { get; set; }

        public CurrencyEnum? Currency { get; set; }

        public QuickDateFilterTypeEnum? QuickDateFilter { get; set; }

        public QuickStatusFilterTypeEnum? QuickStatusFilter { get; set; }

        public List<TransactionStatusEnum> Statuses { get; set; }

        public TransactionTypeEnum? TransactionType { get; set; }

        public JDealTypeEnum? JDealType { get; set; }

        public CardPresenceEnum? CardPresence { get; set; }

        [SwaggerExclude]
        public string ShvaShovarNumber { get; set; }

        [SwaggerExclude]
        public string ShvaTransactionID { get; set; }

        [SwaggerExclude]
        public long? ClearingHouseTransactionID { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }

        public Guid? ConsumerID { get; set; }

        public Guid? CreditCardTokenID { get; set; }

        public string CardNumber { get; set; }

        public string CardOwnerNationalID { get; set; }

        public string CardOwnerName { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }

        /// <summary>
        /// Merchant deal reference
        /// </summary>
        public string DealReference { get; set; }

        public string DealDescription { get; set; }

        [SwaggerExclude]
        public SolekEnum? Solek { get; set; }

        [SwaggerExclude]
        public string CreditCardVendor { get; set; }

        public Guid? BillingDealID { get; set; }

        public SpecialTransactionTypeEnum? SpecialTransactionType { get; set; }

        public bool NotTransmitted { get; set; }
    }
}
