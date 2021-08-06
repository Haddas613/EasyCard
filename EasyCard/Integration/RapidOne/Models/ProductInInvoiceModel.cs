using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RapidOne.Models
{
    [DataContract]
    public class ProductInInvoiceModel
    {
        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "quantity")]
        public int Quantity { get; set; }
        //"unitPrice": {
        //    "priceListCode": 11,
        //    "value": 100,
        //    "currency": "₪"
        //},

        [DataMember(Name = "unitPrice")]
        public UnitPriceModel UnitPrice { get; set; }


        [DataMember(Name = "subtotal")]
        public decimal Subtotal { get; set; }

        [DataMember(Name = "vatPercent")]
        public decimal VatPercent { get; set; }//": 17,

        [DataMember(Name = "vat")]
        public decimal Vat { get; set; }//: 17,

        [DataMember(Name = "total")]
        public decimal Total { get; set; }//": 117,

        [DataMember(Name = "toPay")]

        public decimal ToPay { get; set; }//": 117,

        [DataMember(Name = "discount")]
        public decimal Discount { get; set; }

        [DataMember(Name = "users")]
        public Object[] Users { get; set; }

        [DataMember(Name = "notes")]
        public string Notes { get; set; }

        [DataMember(Name = "disabled")]
        public bool Disabled { get; set; }

        [DataMember(Name = "isDefaultItem")]
        public bool IsDefaultItem { get; set; }

        [DataMember(Name = "isDefaultItemWithVat")]
        public bool IsDefaultItemWithVat { get; set; }

        [DataMember(Name = "rate")]
        public int Rate { get; set; }

        [DataMember(Name = "preventDiscount")]
        public bool PreventDiscount { get; set; }


        // public int priceListId { get; set; }
        // public bool vatPercentDisable { get; set; }
        // public string paid { get; set; }
        //public bool optionalExpanded { get; set; }
        [DataMember(Name = "charge")]
        public bool Charge { get; set; }


        public ProductInInvoiceModel()
        {
            this.Users = new object[0];
            this.Notes = null;
            this.Disabled = false;
            this.Discount = 0;
            this.IsDefaultItem = true;
            this.IsDefaultItemWithVat = true;
            this.Rate = 1;
            this.PreventDiscount = false;
        }
    }
}
