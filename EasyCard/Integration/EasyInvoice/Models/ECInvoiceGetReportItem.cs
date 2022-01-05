using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{
    public class ECInvoiceGetReportItem
    {
        public string transactionDateTime { get; set; }
        public string customerName { get; set; }
        public string documentType { get; set; }
        public string documentCopyUrl { get; set; }
        public int id { get; set; }
         
        public int documentNumber { get; set; }
        public decimal totalGrossAmount { get; set; }
        public decimal totalNetAmount { get; set; }
        public decimal discountAmount { get; set; }
        public decimal totalAmountBeforeDiscount { get; set; }
        public decimal taxAmount { get; set; }
        public decimal taxPercentage { get; set; }
        public decimal totalPaidAmount { get; set; }
        public string deletedAt { get; set; }

        public string[] paymentMethods { get; set; }

    }
}
