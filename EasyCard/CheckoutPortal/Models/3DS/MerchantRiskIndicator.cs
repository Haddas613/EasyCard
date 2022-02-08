using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models._3DS
{
    public class MerchantRiskIndicator
    {
        public string shipIndicator { get; set; }
        public string deliveryTimeframe { get; set; }
        public string deliveryEmailAddress { get; set; }
        public string reorderItemsInd { get; set; }
        public string preOrderPurchaseInd { get; set; }
        public string preOrderDate { get; set; }
        public string giftCardAmount { get; set; }
        public string giftCardCurr { get; set; }
        public string giftCardCount { get; set; }
    }
}
