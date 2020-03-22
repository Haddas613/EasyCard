using System;

namespace Transactions.Business.Entities
{
    public class TransactionHistory
    {
        public long TransactionHistoryID { get; set; }

        public long? PaymentTransactionID { get; set; }

        public DateTime? OperationDate { get; set; }

        public string OperationDoneBy { get; set; }

        public string OperationDoneByID { get; set; }

        /// <summary>
        /// TODO: change to enum
        /// </summary>
        public string OperationCode { get; set; }

        public string OperationDescription { get; set; }

        public string OperationMessage { get; set; }

        /// <summary>
        /// additional details
        /// </summary>
        public string AdditionalDetails { get; set; }

        public string CorrelationId { get; set; }

        public string IntegrationMessageId { get; set; }
    }
}