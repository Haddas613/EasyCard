using CheckoutPortal.Resources;
using Newtonsoft.Json.Linq;
using Shared.Api.Models;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions;

namespace CheckoutPortal.Models
{
    public static class LegacyQueryStringConvertor
    {
        public static LegacyQueryStringModel GetLegacyQueryString(ChargeViewModel request, TransactionResponseAdmin paymentTransaction)
        {
            var culture = new CultureInfo("he");

            ResourceManager rm = new ResourceManager("CardVendor", Assembly.GetExecutingAssembly());

            var shvaDetails = (JObject)paymentTransaction.ShvaTransactionDetails;
            var manpikId = GetInValueManpik(paymentTransaction.CreditCardDetails.CardVendor);
            var solekStr = shvaDetails["solek"]?.ToString();
            var solekId = GetInValueSolek(solekStr);

            return new LegacyQueryStringModel
            {
                TransactionID = paymentTransaction.PaymentTransactionID.ToString(),

                DealID = paymentTransaction.PaymentTransactionID.ToString(),
                StateData = request.StateData,
                Total = paymentTransaction.TotalAmount.ToString("F2"),
                CardOwner = request.Name,
                OwnerEmail = request.Email,
                Id = request.PaymentIntent,

                OkNumber = shvaDetails["shvaAuthNum"]?.ToString(),
                Code = paymentTransaction.ProcessorResultCode.ToString(),

                BusinessName = paymentTransaction.MerchantName,
                Terminal = shvaDetails["shvaTerminalID"]?.ToString(),
                DealNumber = GetDealNumber(shvaDetails["shvaDealID"]?.ToString()),

                CardNumber = paymentTransaction.CreditCardDetails?.CardNumber,
                DealDate = paymentTransaction.TransactionDate.GetValueOrDefault(DateTime.Today).ToString("yyyy-MM-dd"),
                PayNumber = (paymentTransaction.NumberOfPayments - 1).ToString(),
                FirstPay = paymentTransaction.InitialPaymentAmount.ToString("F2"),
                AddPay = paymentTransaction.InstallmentPaymentAmount.ToString("F2"),
                DealTypeOut = null,

                DealType = DealType.ResourceManager.GetString(paymentTransaction.TransactionType.ToString(), culture),
                DealTypeID = GetLegacyDealtypeValue(paymentTransaction.TransactionType).ToString(),

                Currency = GetLegacyCurrency(paymentTransaction.Currency),
                CurrencyID = GetLegacyCurrencyValue(paymentTransaction.Currency).ToString(),

                CardNameID = CardVendor.ResourceManager.GetString(solekId.ToString(), culture),
                CardNameIDCode = ((int)solekId).ToString(),
                Manpik = CardVendor.ResourceManager.GetString(manpikId.ToString(), culture),
                ManpikID = ((int)manpikId).ToString(),
                Mutag = GetMutagStr(paymentTransaction.CreditCardDetails.CardBrand),
                MutagID = paymentTransaction.CreditCardDetails.CardBrand,

                Tz = request.NationalID,
                CardDate = paymentTransaction.CreditCardDetails?.CardExpiration.ToString(),

                Token = paymentTransaction.CreditCardToken,

                PhoneNumber = request.Phone,

                EmvSoftVersion = shvaDetails["emvSoftVersion"]?.ToString(),
                OriginalUID = shvaDetails["shvaDealID"]?.ToString(),
                CompRetailerNum = shvaDetails["compRetailerNum"]?.ToString(),
            };

        }

        internal static string GetDealNumber(string dealNumber)
        {
            if (string.IsNullOrWhiteSpace(dealNumber))
            {
                return null;
            }

            return dealNumber.Length > 20 ? dealNumber.Substring(dealNumber.Length - 20, 20) : dealNumber;
        }

        internal static int GetLegacyCurrencyValue(CurrencyEnum currency)
        {
            int currencyLegacyValue;
            switch (currency)
            {

                case CurrencyEnum.ILS:
                    currencyLegacyValue = 1;
                    break;
                case CurrencyEnum.USD:
                    currencyLegacyValue = 2;
                    break;
                case CurrencyEnum.EUR:
                    currencyLegacyValue = 5;
                    break;
                default:
                    currencyLegacyValue = 1;
                    break;
            }
            return currencyLegacyValue;
        }

        internal static string GetLegacyCurrency(CurrencyEnum currency)
        {
            string currencyLegacyValue;
            switch (currency)
            {

                case CurrencyEnum.ILS:
                    currencyLegacyValue = "שקלים";
                    break;
                case CurrencyEnum.USD:
                    currencyLegacyValue = "דולרים";
                    break;
                case CurrencyEnum.EUR:
                    currencyLegacyValue = "יורו";
                    break;
                default:
                    currencyLegacyValue = "שקלים";
                    break;
            }
            return currencyLegacyValue;
        }

        internal static CardVendorEnum GetInValueManpik(string cardVendor)
        {
            if (string.IsNullOrWhiteSpace(cardVendor))
            {
                return CardVendorEnum.UNKNOWN;
            }

            return (CardVendorEnum)Enum.Parse(typeof(CardVendorEnum), cardVendor, true);
        }

        internal static SolekEnum GetInValueSolek(string cardVendor)
        {
            if (string.IsNullOrWhiteSpace(cardVendor))
            {
                return SolekEnum.UNKNOWN;
            }

            return (SolekEnum)Enum.Parse(typeof(SolekEnum), cardVendor, true);
        }

        internal static string GetMutagStr(string cardBrand)
        {
            string mutag = "";
            switch (cardBrand)
            {
                case "0":
                    mutag = "PL";
                    break;
                case "1":
                    mutag = "מסטרכרד";
                    break;
                case "2":
                    mutag = "ויזה";
                    break;
                case "3":
                    mutag = "מאסטרו";
                    break;
                case "4":
                    mutag = "אמקס";
                    break;
                case "5":
                    mutag = "JCB";
                    break;
                case "6":
                    mutag = "לאומיקארד";
                    break;
                default:
                    break;
            }
            return mutag;
        }

        internal static int GetLegacyDealtypeValue(TransactionTypeEnum transactionType)
        {
            int dealtype = 1;
            switch (transactionType)
            {
                case TransactionTypeEnum.RegularDeal:
                    dealtype = 1;
                    break;
                case TransactionTypeEnum.Installments:
                    dealtype = 8;
                    break;
                case TransactionTypeEnum.Credit:
                    dealtype = 6;
                    break;
                default:
                    break;
            }
            return dealtype;

            /*
              CREDIT_CARD_REGULAR_CREDIT = 1,
              CREDIT_CARD_PLUS_30 = 2,
              CREDIT_CARD_INSTANT_BILLING = 3,
              CREDIT_CARD_CLUB_CREDIT = 4,//never used
              CREDIT_CARD_SUPER_CREDIT = 5,//never used
              CREDIT_CARD_CREDITS = 6,
              CREDIT_CARD_PAYMENTS = 8,
              SOMETHING = 10,
              PAYMENTS_80 = 80,//PaymentsWithCommission?? for upay
              PAYMENTS_90 = 90,
              CREDIT_CARD_INSTALLMENT_CLUB_DEAL = 9,//never used
              PAYPAL = 50,
              SHOTEF_30_UPAY = 1030,//never used
              SHOTEF_60_UPAY = 1060,//never used
              SHOTEF_90_UPAY = 1090//never used
            */
        }
    }
}
