using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.MDBSource
{
    public class PausedDealsPerCustomer
    {
        public string DealID { get; set; }
        public int Month { get; set; }

        /// <summary>
        /// YYYY
        /// </summary>
        public int Year { get; set; }
        public string Note { get; set; }
    }
}
