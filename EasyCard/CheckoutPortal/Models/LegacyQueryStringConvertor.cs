using CheckoutPortal.Resources;
using Merchants.Business.Entities.Terminal;
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

namespace CheckoutPortal.Models
{
    public static class LegacyQueryStringConvertor
    {
        public static object GetLegacyQueryString(OperationResponse result, ChargeViewModel request, LegacyQueryStringModel paymentTransaction, Terminal terminalDetails)
        {
            var culture = new CultureInfo("he");


            //string message = CardVendor.ResourceManager.GetString("NameOfKey", culture);
            ResourceManager rm = new ResourceManager("CardVendor", Assembly.GetExecutingAssembly());
            return new
            {
                transactionID = result.EntityUID,
                StateData = request.StateData,
                Total = request.TotalAmount,
                CardOwner = request.Name,
                OwnerEmail = request.Email,
                Id = "TODO",//request.    redirectpaymentpageid
                OkNumber = paymentTransaction.OkNumber,
                Code = paymentTransaction.Code,
                DealID = paymentTransaction.DealID,
                BusinessName = terminalDetails.Label,
                Terminal = paymentTransaction.Terminal,
                DealNumber = paymentTransaction.DealNumber.Length > 20 ? paymentTransaction.DealNumber.Substring(paymentTransaction.DealNumber.Length - 20, 20) : paymentTransaction.DealNumber,//
                CardNumber = paymentTransaction.CardNumber,
                DealDate = paymentTransaction.DealDate,
                PayNumber = paymentTransaction.PayNumber,
                FirstPay = paymentTransaction.FirstPay,
                AddPay = paymentTransaction.AddPay,
                DealTypeOut = paymentTransaction.DealTypeOut,
                DealType = DealType.ResourceManager.GetString(paymentTransaction.DealType, culture),
                Currency = paymentTransaction.Currency,
                CardNameID = CardVendor.ResourceManager.GetString(paymentTransaction.CardNameID, culture) ,
                Manpik = CardVendor.ResourceManager.GetString(paymentTransaction.Manpik, culture),
                Mutag = paymentTransaction.Mutag,
                DealTypeID = paymentTransaction.DealTypeID,
                CurrencyID = paymentTransaction.CurrencyID,
                CardNameIDCode = paymentTransaction.CardNameIDCode,
                ManpikID = paymentTransaction.ManpikID,
                MutagID = paymentTransaction.MutagID, 
                Tz = request.NationalID,
                CardDate = paymentTransaction.CardDate,
                Token = paymentTransaction.Token,
                phoneNumber = request.Phone,
                EmvSoftVersion = paymentTransaction.EmvSoftVersion,
                OriginalUID = paymentTransaction.OriginalUID,
                CompRetailerNum = paymentTransaction.CompRetailerNum
            };

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

        internal static int GetInValueManpik(string cardVendor)
        {
           return (int)(CardVendorEnum)Enum.Parse(typeof(CardVendorEnum), cardVendor);
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
            int dealtype=1;
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
             *  *INT
            * CREDIT_CARD_REGULAR_CREDIT = 1,
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
             * */

        }
    }
}
