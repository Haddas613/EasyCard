using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models._3DS
{
    public class AuthenticationRequest
    {
        public string messageType { get; set; }
        public string threeDSCompInd { get; set; }
        public string threeDSRequestorID { get; set; }
        public string threeDSRequestorName { get; set; }
        public string threeDSRequestorAuthenticationInd { get; set; }
        public string threeDSRequestorURL { get; set; }
        public string threeDSServerRefNumber { get; set; }
        public string threeDSServerOperatorID { get; set; }
        public string threeDSServerTransID { get; set; }
        public string threeDSServerURL { get; set; }
        public string acctType { get; set; }
        public string acquirerBIN { get; set; }
        public string acquirerMerchantID { get; set; }
        public string addrMatch { get; set; }
        public string browserAcceptHeader { get; set; }
        public string browserIP { get; set; }
        public bool browserJavaEnabled { get; set; }
        public string browserLanguage { get; set; }
        public string browserColorDepth { get; set; }
        public string browserScreenHeight { get; set; }
        public string browserScreenWidth { get; set; }
        public string browserTZ { get; set; }
        public string browserUserAgent { get; set; }
        public string cardExpiryDate { get; set; }
        public AcctInfo acctInfo { get; set; }

        public string acctNumber { get; set; }
        public string billAddrCity { get; set; }
        public string billAddrCountry { get; set; }
        public string billAddrLine1 { get; set; }
        public string billAddrLine2 { get; set; }
        public string billAddrLine3 { get; set; }
        public string billAddrPostCode { get; set; }
        public string billAddrState { get; set; }
        public string email { get; set; }
        public Phone homePhone { get; set; }
        public Phone mobilePhone { get; set; }

        public string cardholderName { get; set; }
        public string shipAddrCity { get; set; }
        public string shipAddrCountry { get; set; }
        public string shipAddrLine1 { get; set; }
        public string shipAddrLine2 { get; set; }
        public string shipAddrLine3 { get; set; }
        public string shipAddrPostCode { get; set; }
        public string shipAddrState { get; set; }
        public Phone workPhone { get; set; }

        public string deviceChannel { get; set; }
        public bool payTokenInd { get; set; }
        public string mcc { get; set; }
        public string merchantCountryCode { get; set; }
        public string merchantName { get; set; }
        public MerchantRiskIndicator merchantRiskIndicator { get; set; }

        public string messageCategory { get; set; }
        public string messageVersion { get; set; }
        public string purchaseAmount { get; set; }
        public string purchaseExponent { get; set; }
        public string purchaseCurrency { get; set; }
        public string purchaseDate { get; set; }
        public string recurringExpiry { get; set; }
        public string recurringFrequency { get; set; }
        public string transType { get; set; }
        public string notificationURL { get; set; }
        public BroadInfo broadInfo { get; set; }

    }
}
