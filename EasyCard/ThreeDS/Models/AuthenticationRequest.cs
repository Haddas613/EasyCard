using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Models
{
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class AuthenticationRequest
	{
		public string Retailer { get; set; }//shva client code
		public string PspID { get; set; }// Distributer ID
		public string UserName { get; set; }
		public string Password { get; set; }
		public string MerchantURL { get; set; }
		public string NotificationURL { get; set; }
		public string ThreeDSServerTransID { get; set; }
		public string AcctNumber { get; set; } // required (card number)
		public string MessageType { get; set; }

		public string cardExpiryDateYear { get; set; }

		public string cardExpiryDateMonth { get; set; }

		public string AcctType { get; set; }// try just with parameters until here leave all the rest empty

		public string AcquirerBin { get; set; }
		public string AcquirerMerchantId { get; set; }
		public string Brand { get; set; }

		public string acctInfo_chAccAgeInd { get; set; }

		public string acctInfo_chAccDate { get; set; }

		public string acctInfo_chAccChangeInd { get; set; }
		public string acctInfo_chAccChange { get; set; }

		public string acctInfo_chAccPwChangeInd { get; set; }
		public string acctInfo_chAccPwChange { get; set; }
		public string acctInfo_shipAddressUsageInd { get; set; }
		public string acctInfo_shipAddressUsage { get; set; }
		public string acctInfo_txnActivityDay { get; set; }
		public string acctInfo_txnActivityYear { get; set; }

		public string acctInfo_provisionAttemptsDay { get; set; }
		public string acctInfo_nbPurchaseAccount { get; set; }
		public string acctInfo_suspiciousAccActivity { get; set; }
		public string acctInfo_shipNameIndicator { get; set; }
		public string acctInfo_paymentAccInd { get; set; }
		public string acctInfo_paymentAccAge { get; set; }


		public string schemeId { get; set; }
		public string payTokenInd { get; set; }
		/// <summary>
		/// optional
		/// </summary>
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
		public string purchaseInstalData { get; set; }
		public string merchantRiskIndicator_shipIndicator { get; set; }
		public string merchantRiskIndicator_deliveryTimeframe { get; set; }
		public string merchantRiskIndicator_deliveryEmailAddress { get; set; }
		public string merchantRiskIndicator_reorderItemsInd { get; set; }
		public string merchantRiskIndicator_preOrderPurchaseInd { get; set; }
		public string merchantRiskIndicator_preOrderDate { get; set; }
		public string merchantRiskIndicator_giftCardAmount { get; set; }
		public string merchantRiskIndicator_giftCardCurr { get; set; }
		public string merchantRiskIndicator_giftCardCount { get; set; }
		public string purchaseAmount { get; set; }

		public string PurchaseCurrency { get; set; }

		public string purchaseExponent { get; set; }

		public string PurchaseDate { get; set; }

		public string recurringExpiry { get; set; }
		public string recurringFrequency { get; set; }
		public string transType { get; set; }

		public string Merchant_mcc { get; set; }

		public string merchantCountryCode { get; set; }

		public string MerchantName { get; set; }

		public string threeDSRequestorId { get; set; }
		public string threeDSRequestorName { get; set; }

		public string BrowserAcceptHeader { get; set; }

		public string browserIP { get; set; }
		public bool browserJavaEnabled { get; set; }

		public string BrowserLanguage { get; set; }

		public string BrowserColorDepth { get; set; }
		public string BrowserScreenHeight { get; set; }
		public string BrowserScreenWidth { get; set; }

		public string BrowserTZ { get; set; }

		public string BrowserUserAgent { get; set; }
		public string challengeWindowSize { get; set; }
	}
}
