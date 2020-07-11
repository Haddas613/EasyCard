using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class DealItem
    {
        public Guid? ItemID { get; set; }

        public string ItemName { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }
    }
}
