using System;
using System.Collections.Generic;
using System.Text;

namespace Nayax.Models
{
    public class Phase2ResponseBody
    {
        /// <summary>
        /// o OK
        /// </summary>
        public string statusCode { get; set; }
        public string statusMessage { get; set; }
        public double amount { get; set; }
        public int mutag { get; set; }
        public int manpik { get; set; }
        public int solek { get; set; }
        public string cardNumber { get; set; }
        public int tranType { get; set; }
        public int posEntryMode { get; set; }
        public int minCreditPayments { get; set; }
        public int maxCreditPayments { get; set; }
        public int minCreditAmount { get; set; }
        public int creditTerms { get; set; }
        //public string cardName { get; set; }
        public string uid { get; set; }
        public string vuid { get; set; }
        public string issuerAuthNum { get; set; }
        public string rrn { get; set; }
        public string sysTraceNumber { get; set; }//מספר שובר
        public string acquirerMerchantID { get; set; }
        public string expDate { get; set; }
        //For Emv Desktop
        public int authCodeManpik { get; set; }
        public string cardName { get; set; }
        public string camutagNamerdName { get; set; }
        public string currency { get; set; }
        public string retailerId { get; set; }
    }
}
