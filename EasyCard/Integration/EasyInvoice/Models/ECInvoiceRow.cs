using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace EasyInvoice.Models
{
    [DataContract]
    public class ECInvoiceRow
    {
        [DataMember(Name = "sku")]
        public string Sku { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "price")]
        public decimal? Price { get; set; }

        [DataMember(Name = "priceNet")]
        public decimal? PriceNet { get; set; }

        [DataMember(Name = "taxAmount")]
        public decimal? TaxAmount { get; set; }

        [DataMember(Name = "quantity")]
        public int Quantity { get; set; }

        [DataMember(Name = "totalAmount")]
        public decimal? TotalAmount { get; set; }

        [DataMember(Name = "totalNetAmount")]
        public decimal? TotalNetAmount { get; set; }

        [DataMember(Name = "totalTaxAmount")]
        public decimal? TotalTaxAmount { get; set; }
    }
}
