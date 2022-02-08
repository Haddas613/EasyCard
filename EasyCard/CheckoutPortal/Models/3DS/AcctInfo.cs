using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models._3DS
{
    public class AcctInfo
    {

        public string chAccAgeInd { get; set; }
 public string chAccDate { get; set; }
        public string chAccChangeInd { get; set; }
        public string chAccChange { get; set; }
        public string chAccPwChangeInd{ get; set; }
 public string chAccPwChange { get; set; }
        public string shipAddressUsageInd { get; set; }
        public string shipAddressUsage { get; set; }
        public string txnActivityDay { get; set; }
        public string txnActivityYear { get; set; }
        public string provisionAttemptsDay { get; set; }
        public string nbPurchaseAccount { get; set; }
        public string suspiciousAccActivity { get; set; }
        public string shipNameIndicator { get; set; }
        public string paymentAccInd { get; set; }
        public string paymentAccAge { get; set; }
    }
}
