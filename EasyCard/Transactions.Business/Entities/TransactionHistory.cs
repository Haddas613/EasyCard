using Shared.Business.Security;
using Shared.Helpers;
using System;
using Transactions.Shared.Enums;

namespace Transactions.Business.Entities
{
    public class TransactionHistory: IAuditEntity
    {
        public TransactionHistory()
        {
            //Terminals = new HashSet<Merchants.Business.Entities.Terminal.Terminal>();
            OperationDate = DateTime.UtcNow;
            TransactionHistoryID = Guid.NewGuid().GetSequentialGuid(OperationDate.Value);
        }

        public Guid TransactionHistoryID { get; set; }

        public Guid? PaymentTransactionID { get; set; }

        public DateTime? OperationDate { get; set; }

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        /// <summary>
        /// TODO: change to enum
        /// </summary>
        public TransactionOperationCodesEnum OperationCode { get; set; }

        public string OperationDescription { get; set; }

        public string OperationMessage { get; set; }

        public string CorrelationId { get; set; }

        public string IntegrationMessageId { get; set; }

        public string SourceIP { get; set; }
    }
}