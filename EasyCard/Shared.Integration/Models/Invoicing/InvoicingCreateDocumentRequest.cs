﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared.Integration.Models.Invoicing
{
    [DataContract]
    public class InvoicingCreateDocumentRequest
    {

        [DataMember(Name = "transactionDateTime")]
        public string TransactionDateTime { get; set; }

        [DataMember(Name = "documentType")]
        public string DocumentType { get; set; }

        [DataMember(Name = "customerName")]
        public string CustomerName { get; set; }

        [DataMember(Name = "customerTaxId")]
        public string CustomerTaxId { get; set; }

        [DataMember(Name = "customerAddress")]
        public CustomerAddress CustomerAddress { get; set; }

        [DataMember(Name = "customerPhoneNumber")]
        public string CustomerPhoneNumber { get; set; }

        [DataMember(Name = "customerEmail")]
        public string CustomerEmail { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "totalAmount")]
        public decimal? TotalAmount { get; set; }

        [DataMember(Name = "totalNetAmount")]
        public decimal? TotalNetAmount { get; set; }

        [DataMember(Name = "taxAmount")]
        public decimal? TaxAmount { get; set; }

        [DataMember(Name = "taxPercentage")]
        public decimal? TaxPercentage { get; set; }

        [DataMember(Name = "discountAmount")]
        public decimal? DiscountAmount { get; set; }

        [DataMember(Name = "totalAmountBeforeDiscount")]
        public decimal? TotalAmountBeforeDiscount { get; set; }

        [DataMember(Name = "totalPaidAmount")]
        public decimal? TotalPaidAmount { get; set; }

        [DataMember(Name = "sendEmail")]
        public bool SendEmail { get; set; }

        [DataMember(Name = "rows")]
        public IList<Row> Rows { get; set; }

        [DataMember(Name = "payments")]
        public IList<Payment> Payments { get; set; }

        [DataMember(Name = "keyStorePassword")]
        public string KeyStorePassword { get; set; }
    }
}