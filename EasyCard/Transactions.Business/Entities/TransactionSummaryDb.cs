using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Shared.Enums;

namespace Transactions.Business.Entities
{
    public class TransactionSummaryDb
    {
        public Guid PaymentTransactionID { get; set; }

        /// <summary>
        /// Legal transaction day
        /// </summary>
        public DateTime? TransactionDate { get; set; }

        public Guid TerminalID { get; set; }

        public Guid MerchantID { get; set; }

        public decimal TransactionAmount { get; set; }

        public TransactionTypeEnum TransactionType { get; set; }

        public CurrencyEnum Currency { get; set; }

        public DateTime? TransactionTimestamp { get; set; }

        /// <summary>
        /// Processing status
        /// </summary>
        public TransactionStatusEnum Status { get; set; }

        public SpecialTransactionTypeEnum SpecialTransactionType { get; set; }

        /// <summary>
        /// J3, J4, J5
        /// </summary>
        public JDealTypeEnum JDealType { get; set; }

        /// <summary>
        /// Rejection Reason
        /// </summary>
        public RejectionReasonEnum? RejectionReason { get; set; }

        /// <summary>
        /// Telephone deal or Regular (megnetic)
        /// </summary>
        public CardPresenceEnum CardPresence { get; set; }

        public string CardOwnerName { get; set; }

        public int NumberOfRecords { get; set; }
    }
}
