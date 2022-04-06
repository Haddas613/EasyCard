using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models.Invoicing
{
    public enum InvoiceBillingTypeEnum : short
    {
        /// <summary>
        /// Without transaction and billing
        /// </summary>
        Invoice = 0,

        /// <summary>
        /// Transaction but has no billing
        /// </summary>
        TransactionInvoice = 1,

        /// <summary>
        /// Billing's invoice. But billing is without transactions (invoice only)
        /// </summary>
        InvoiceOnlyBilling = 2,

        /// <summary>
        /// Billing's invoice. Billing payment type is credit card
        /// </summary>
        CreditCardBilling = 3,

        /// <summary>
        /// Billing's invoice. Billing payment type is bank
        /// </summary>
        BankBilling = 4,
    }
}
