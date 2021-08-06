using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RapidOneInvoices.Models
{
    [DataContract]
    public class RapidInvoiceCreateDocumentRequest
    {
        [DataMember(Name = "company")]
        public string Company { get; set; }

        [DataMember(Name = "invoiceTypeId")]
        public string InvoiceTypeId { get; set; }

        [DataMember(Name = "invoiceDate")]
        public DateTime? InvoiceDate { get; set; }

        [DataMember(Name = "dueDate")]
        public DateTime? DueDate { get; set; }

        [DataMember(Name = "branchId")]
        public int BranchId { get; set; }

        [DataMember(Name = "departmentId")]
        public int DepartmentId { get; set; }

        [DataMember(Name = "items")]

        public List<ProductInInvoiceModel> Items { get; set; }

        [DataMember(Name = "paymentMethods")]
        public PaymentMethodModel PaymentMethods { get; set; }

        [DataMember(Name = "subtotal")]
        public decimal Subtotal { get; set; }

        [DataMember(Name = "discount")]
        public decimal Discount { get; set; }

        [DataMember(Name = "vat")]
        public decimal Vat { get; set; } //17
        [DataMember(Name = "total")]
        public decimal Total { get; set; }// 117
        [DataMember(Name = "toPay")]
        public decimal ToPay { get; set; }// 117,

        [DataMember(Name = "customerCode")]
        public string CustomerCode { get; set; }//": "RMA_1102",

        [DataMember(Name = "customerName")]
        public string CustomerName { get; set; }

        [DataMember(Name = "customerEmail")]
        public string CustomerEmail { get; set; }

        [DataMember(Name = "customerCell")]
        public string CustomerCell { get; set; }

        public RapidInvoiceCreateDocumentRequest()
        {
            this.InvoiceTypeId = null;//"InvoiceReceipt";
        }
    }
}
