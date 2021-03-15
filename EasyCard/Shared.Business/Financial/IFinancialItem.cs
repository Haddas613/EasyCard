using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Business.Financial
{
    public interface IFinancialItem
    {
        public decimal Amount { get; set; }

        public decimal VATRate { get; set; }

        public decimal VATTotal { get; set; }

        public decimal NetTotal { get; set; }
    }
}
