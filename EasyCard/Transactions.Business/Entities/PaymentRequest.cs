using Shared.Business;
using Shared.Business.Financial;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Transactions.Shared.Enums;

namespace Transactions.Business.Entities
{
    public class PaymentRequest : IEntityBase<Guid>, IAuditEntity, IFinancialItem
    {
        public PaymentRequest()
        {
            PaymentRequestTimestamp = DateTime.UtcNow;
            PaymentRequestID = Guid.NewGuid().GetSequentialGuid(PaymentRequestTimestamp.Value);
            DealDetails = new DealDetails();
            InvoiceDetails = new InvoiceDetails();
        }

        /// <summary>
        /// Primary reference
        /// </summary>
        public Guid PaymentRequestID { get; set; }

        /// <summary>
        /// Date-time when deal created initially in UTC
        /// </summary>
        public DateTime? PaymentRequestTimestamp { get; set; }

        /// <summary>
        /// Due date
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Terminal
        /// </summary>
        public Guid? TerminalID { get; set; }

        /// <summary>
        /// Merchant
        /// </summary>
        public Guid? MerchantID { get; set; }

        public InvoiceDetails InvoiceDetails { get; set; }

        /// <summary>
        /// Create document for transaction
        /// </summary>
        public bool IssueInvoice { get; set; }

        /// <summary>
        /// Processing status
        /// </summary>
        public PaymentRequestStatusEnum Status { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Number Of payments (cannot be more than 999)
        /// </summary>
        public int NumberOfPayments { get; set; }

        /// <summary>
        /// This amount
        /// </summary>
        public decimal PaymentRequestAmount { get; set; }

        [NotMapped]
        public decimal Amount { get => PaymentRequestAmount; set => PaymentRequestAmount = value; }

        /// <summary>
        /// Will be used for invoice
        /// </summary>
        public decimal VATRate { get; set; }

        public decimal VATTotal { get; set; }

        public decimal NetTotal { get; set; }

        public string CardOwnerName { get; set; }

        public string CardOwnerNationalID { get; set; }

        /// <summary>
        /// Deal information
        /// </summary>
        public DealDetails DealDetails { get; set; }

        /// <summary>
        /// Date-time when transaction status updated
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Concurrency key
        /// </summary>
        public byte[] UpdateTimestamp { get; set; }

        public Guid GetID()
        {
            return PaymentRequestID;
        }

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public string CorrelationId { get; set; }

        public string SourceIP { get; set; }

        public Guid? PaymentTransactionID { get; set; }

        public string RequestSubject { get; set; }

        public string FromAddress { get; set; }

        // TODO: recalculate items and fill default SKU
    }
}
