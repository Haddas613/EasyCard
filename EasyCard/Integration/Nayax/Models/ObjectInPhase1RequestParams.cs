using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nayax.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ObjectInPhase1RequestParams
    {
        /// <summary>
        /// Amount in cents 100 = shekel
        /// </summary>
        public double amount { get; set; }
        public string vuid { get; set; }
        /// <summary>
        ///  (ISO 4217)  376 for shekel
        /// </summary>
        public string currency { get; set; }
        /// <summary>
        /// 1 regular
        /// 3 immediate
        /// 6 credit
        /// 8 payments
        /// </summary>
        public int? creditTerms { get; set; }
        /// <summary>
        /// number of credit payments
        /// </summary>
       
        public int? creditPayments { get; set; }
        public int? payments { get; set; }
        public int? firstPaymentAmount { get; set; }
        public int? otherPaymentAmount { get; set; }
        public string cardHolderID { get; set; }
        /// <summary>
        /// 1 - Normal
        ///2 - Pre Auth
        ///3 - Pre Auth Completion
        ///4 - Pre Auth Cancelation
        ///5 - Cancel
        ///6 - Authorized transaction
        ///All other values
        ///are errors TBD
        /// </summary>
        public int tranCode { get; set; }
        /// <summary>
        /// 1 regular
        /// 53 refund
        /// </summary>
        public int tranType { get; set; }

        public string sysTraceNumber { get; set; }
    }
}
