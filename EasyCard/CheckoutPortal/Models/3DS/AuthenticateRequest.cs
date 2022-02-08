using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models._3DS
{
    public class AuthenticateRequest
    {
        public string Retailer { get; set; }
        public string PspID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string MerchantUrl { get; set; }
        public string notificationURL { get; set; }
        public string threeDSServerTransID { get; set; }
        public string threeDSRequestorAuthenticationInd { get; set; }
        public string threeDSReqAuthMethod { get; set; }
        public string threeDSRequestorChallengeInd { get; set; }
        public string threeDSRequestorPriorAuthenticationInfo_threeDSReqPriorRef { get; set; }
        public string threeDSRequestorPriorAuthenticationInfo_threeDSReqPriorAuthMethod { get; set; }

        public string threeDSRequestorPriorAuthenticationInfo_threeDSReqPriorAuthTimestamp { get; set; }

        public string threeDSRequestorPriorAuthenticationInfo_threeDSReqPriorAuthData { get; set; }
        public string acctType { get; set; }
        public string acctInfo_chAccAgeInd { get; set; }
        public string acctInfo_chAccDate { get; set; }
        //acctInfo_chAccChangeInd": null, 
        public string acctInfo_chAccChange { get; set; }
        public string acctInfo_chAccPwChangeInd { get; set; }
        public string acctInfo_chAccPwChange { get; set; }
        public string acctInfo_shipAddressUsageInd { get; set; }
        public string acctInfo_shipAddressUsage { get; set; }
        public int acctInfo_txnActivityDay { get; set; }
        public int acctInfo_txnActivityYear { get; set; }
        public int acctInfo_provisionAttemptsDay { get; set; }
        public int acctInfo_nbPurchaseAccount { get; set; }
        public string acctInfo_suspiciousAccActivity { get; set; }
        public string acctInfo_shipNameIndicator { get; set; }
        public string acctInfo_paymentAccInd { get; set; }
        public string acctInfo_paymentAccAge { get; set; }
        public string acctNumber { get; set; }
        public int cardExpiryDateYear { get; set; }
        public int cardExpiryDateMonth { get; set; }
        public string schemeId { get; set; }
        public bool payTokenInd { get; set; }
        public string addrMatch { get; set; }
        public string billAddrCity { get; set; }
        public string billAddrCountry { get; set; }
        public string billAddrLine1 { get; set; }
        public string billAddrLine2 { get; set; }
        public string billAddrLine3 { get; set; }
        public string billAddrPostCode { get; set; }

        public string billAddrState { get; set; }
        public string email { get; set; }
        public string homePhone_cc { get; set; }
        public string homePhone_subscriber { get; set; }
        public string mobilePhone_cc { get; set; }
        public string mobilePhone_subscriber { get; set; }
        public string workPhone_cc { get; set; }
        public string workPhone_subscriber { get; set; }
        public string cardholderName { get; set; }
        public string shipAddrCity { get; set; }
        public string shipAddrCountry { get; set; }
        public string shipAddrLine1 { get; set; }
        public string shipAddrLine2 { get; set; }
        public string shipAddrLine3 { get; set; }
        public string shipAddrPostCode { get; set; }
        public string shipAddrState { get; set; }
        public int purchaseInstalData { get; set; }
        public string merchantRiskIndicator_shipIndicator { get; set; }
        public string merchantRiskIndicator_deliveryTimeframe { get; set; }
        public string merchantRiskIndicator_deliveryEmailAddress { get; set; }
        public string merchantRiskIndicator_reorderItemsInd { get; set; }
        public string merchantRiskIndicator_preOrderPurchaseInd { get; set; }
        public string merchantRiskIndicator_preOrderDate { get; set; }
        public int merchantRiskIndicator_giftCardAmount { get; set; }
        public string merchantRiskIndicator_giftCardCurr { get; set; }
        public int merchantRiskIndicator_giftCardCount { get; set; }
        public int purchaseAmount { get; set; }
        public string purchaseCurrency { get; set; }
        public int purchaseExponent { get; set; }
        public string purchaseDate { get; set; }
        public string recurringExpiry { get; set; }
        public int recurringFrequency { get; set; }
        public string transType { get; set; }
        public string merchant_mcc { get; set; }
        public string merchantCountryCode { get; set; }
        public string merchantName { get; set; }
        public string threeDSRequestorId { get; set; }
        public string threeDSRequestorName { get; set; }
        public string browserAcceptHeader { get; set; }
        public string browserIP { get; set; }
        public bool browserJavaEnabled { get; set; }
        public string browserLanguage { get; set; }
        public string browserColorDepth { get; set; }
        public string browserScreenHeight { get; set; }
        public string browserScreenWidth { get; set; }
        public string browserTZ { get; set; }
        public string browserUserAgent { get; set; }
        public string challengeWindowSize { get; set; }

    }
}
