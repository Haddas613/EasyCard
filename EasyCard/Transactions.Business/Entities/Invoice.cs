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
using Transactions.Shared.Models;

namespace Transactions.Business.Entities
{
    public class Invoice : IEntityBase<Guid>, IAuditEntity, IFinancialItem, ITerminalEntity, IMerchantEntity
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
        /// Invoice reference in invoicing system
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Date-time when entity created initially in UTC
        /// </summary>
        public DateTime? InvoiceTimestamp { get; set; }

        /// <summary>
        /// Legal invoice day
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// Terminal
        /// </summary>
        public Guid TerminalID { get; set; }

        /// <summary>
        /// Merchant
        /// </summary>
        public Guid MerchantID { get; set; }

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
        /// This invoice amount
        /// </summary>
        public decimal InvoiceAmount { get; set; }

        [NotMapped]
        public decimal Amount { get => InvoiceAmount; set => InvoiceAmount = value; }

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
        /// Date-time when entity status updated
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

        public string DownloadUrl { get; set; }

        public string CopyDonwnloadUrl { get; set; }

        /// <summary>
        /// Credit card information
        /// </summary>
        public CreditCardDetails CreditCardDetails { get; set; }

        public DocumentOriginEnum DocumentOrigin { get; set; }

        // TODO: calculate items
        [Obsolete]
        public void Calculate()
        {
        }
    }
}
