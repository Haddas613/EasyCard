using Shared.Api.Models;
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

        public Guid? MerchantID { get; set; }

        [Range(0, double.PositiveInfinity)]
        public decimal? AmountFrom { get; set; }

        [Range(0, double.PositiveInfinity)]
        public decimal? AmountTo { get; set; }

        public CurrencyEnum? Currency { get; set; }

        public QuickTimeFilterTypeEnum? QuickTimeFilter { get; set; }

        public QuickStatusFilterTypeEnum? QuickStatusFilter { get; set; }

        public List<TransactionStatusEnum> Statuses { get; set; }

        public TransactionTypeEnum? Type { get; set; }

        public JDealTypeEnum? JDealType { get; set; }

        public CardPresenceEnum? CardPresence { get; set; }

        public string ShvaShovarNumber { get; set; }

        public string ShvaTransactionID { get; set; }

        public long? ClearingHouseTransactionID { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public DateFilterTypeEnum? DateType { get; set; }
    }
}
