using Common.Models.MDBSource;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
    public class DataFromMDBFile
    {
        public BillingSettings BillingSetting { get; set; }

        public IEnumerable<Customer> Customers { get; set; }

        public IEnumerable<NotActiveProduct> NotActiveProducts { get; set; }

        public IEnumerable<Product> Products { get; set; }

        public IEnumerable<ProductPerCustomer> ProductsPerCustomer{get;set;}

        public IEnumerable<RowndsProductsPerCustomer> RowndsProductsPerCustomer { get; set; }
    }
}
