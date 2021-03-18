using System;
using System.Collections.Generic;
using System.Text;

namespace Nayax.Models
{
    public class ObjectInPhase2RequestParams
    {
        public string vuid { get; set; }
        /// <summary>
        /// number of credit payments
        /// </summary>
        public int creditPayments { get; set; }
        /// <summary>
        /// 1 regular
        /// 3 immediate
        /// 6 credit
        /// 8 payments
        /// </summary>
        public int creditTerms { get; set; }
        public int payments { get; set; }
        public int firstPaymentAmount { get; set; }
        public int otherPaymentAmount { get; set; }
    }
}
