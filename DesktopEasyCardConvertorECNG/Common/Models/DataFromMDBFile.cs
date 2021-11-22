using Common.Models.MDBSource;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
    public class DataFromMDBFile
    {
        public BillingSettings BillingSetting { get; set; }
        public List<Customer> Customers { get; set; }

        public List<NotActiveProduct> NotActiveProducts { get; set; }

        public List<Product> Products { get; set; }

        public List<ProductPerCustomer> ProductsPerCustomer{get;set;}

        public List<RowndsProductsPerCustomer> RowndsProductsPerCustomer { get; set; }

    }
}
