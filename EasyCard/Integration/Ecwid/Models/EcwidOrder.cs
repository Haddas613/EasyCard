using Ecwid.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecwid.Models
{
    public class EcwidOrder
    {
        public string Id { get; set; }

        public string ReferenceTransactionId { get; set; }

        public decimal RefundedAmount { get; set; }

        /// <summary>
        /// Order subtotal. Includes the sum of all products' cost in the order
        /// </summary>
        public decimal SubTotal { get; set; }

        /// <summary>
        /// Order total cost. Includes shipping, taxes, discounts, etc.
        /// </summary>
        public decimal Total { get; set; }
        public decimal Discount { get; set; }

        public string Email { get; set; }

        /// <summary>
        /// E.g: CUSTOM_PAYMENT_APP-mollie-pg
        /// </summary>
        public string PaymentModule { get; set; }

        /// <summary>
        /// Credit or debit card (Mollie)
        /// </summary>
        public string PaymentMethod { get; set; }

        public decimal Tax { get; set; }
        public bool CustomerTaxExempt { get; set; }
        public string CustomerTaxId { get; set; }

        public string IpAddress { get; set; }

        public EcwidPaymentStatusEnum PaymentStatus { get; set; }
        public EcwidFulfillmentStatusEnum FulfillmentStatus { get; set; }

        public string OrderComments { get; set; }

        public string CustomerId { get; set; }

        public DateTime CreateDate { get; set; }

        //TODO
        public IEnumerable<EcwidOrderItem> Items { get; set; }

        public EcwidAddressDetails BillingPerson { get; set; }

        public EcwidAddressDetails ShippingPerson { get; set; }
    }
}
