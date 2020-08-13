using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models.Invoicing
{
    public class InvoicingCreateDocumentRequest
    {
        public InvoicingCreateDocumentRequest()
        {
            this.DealDetails = new DealDetails();
            this.CreditCardDetails = new CreditCardDetails();
            this.InstallmentDetails = new InstallmentDetails();
        }

        /// <summary>
        /// Invoice date
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// Invoice details
        /// </summary>
        public InvoiceDetails InvoiceDetails { get; set; }

        /// <summary>
        /// EasyCard terminal reference
        /// </summary>
        public Guid TerminalID { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Deal information (optional)
        /// </summary>
        public DealDetails DealDetails { get; set; }

        /// <summary>
        /// Credit card details
        /// </summary>
        public CreditCardDetailsBase CreditCardDetails { get; set; }

        /// <summary>
        /// Invoice amount (should be omitted in case of installment deal)
        /// </summary>
        public decimal? InvoiceAmount { get; set; }

        /// <summary>
        /// Tax rate (VAT)
        /// </summary>
        public decimal? TaxRate { get; set; }

        /// <summary>
        /// Tax amount
        /// </summary>
        public decimal? TaxAmount { get; set; }

        /// <summary>
        /// Installment payments details (should be omitted in case of regular deal)
        /// </summary>
        public InstallmentDetails InstallmentDetails { get; set; }
    }
}
