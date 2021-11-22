using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.MDBSource
{
    public class ProductPerCustomer
    {
        public string DealID { get; set; }
        public string DealText { get; set; }
        public decimal ProdSum { get; set; }
        public string RivID { get; set; }
        public int DealCount { get; set; }

        public string RivCode { get; set; }
    }
}
