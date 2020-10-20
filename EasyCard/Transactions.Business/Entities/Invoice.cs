using Shared.Business;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Shared.Enums;
using Transactions.Shared.Models;

namespace Transactions.Business.Entities
{
    public class Invoice : IEntityBase<Guid>, IAuditEntity
    {
        public Invoice()
        {
            InvoiceTimestamp = DateTime.UtcNow;
            InvoiceID = Guid.NewGuid().GetSequentialGuid(InvoiceTimestamp.Value);
            DealDetails = new DealDetails();
            InvoiceDetails = new InvoiceDetails();
            InvoiceDate = TimeZoneInfo.ConvertTimeFromUtc(InvoiceTimestamp.Value, UserCultureInfo.TimeZone).Date;
        }

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
        /// Terminal
        /// </summary>
        public Guid? TerminalID { get; set; }

        /// <summary>
        /// Merchant
        /// </summary>
        public Guid? MerchantID { get; set; }

        /// <summary>
        /// EasyInvoice or RapidOne
        /// </summary>
        public long? InvoicingID { get; set; }

        public InvoiceDetails InvoiceDetails { get; set; }

        /// <summary>
        /// Processing status
        /// </summary>
        public InvoiceStatusEnum Status { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Number Of payments (cannot be more than 999)
        /// </summary>
        public int NumberOfPayments { get; set; }

        /// <summary>
        /// This invoice amount
        /// </summary>
        public decimal InvoiceAmount { get; set; }

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
            return InvoiceID;
        }

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public string CorrelationId { get; set; }

        public string SourceIP { get; set; }

        public Guid? PaymentTransactionID { get; set; }
    }
}
