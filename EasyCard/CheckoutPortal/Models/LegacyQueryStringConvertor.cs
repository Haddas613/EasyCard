using CheckoutPortal.Resources;
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


         //   CreateMap<PaymentTransaction, LegacyQueryStringModel>()
         //       //  //DealTypeOut = paymentTransaction.DealTypeOut,//empty
         //       .ForMember(q => q.Code, src => src.MapFrom(src => src.ProcessorResultCode))
         //       .ForMember(q => q.OkNumber, src => src.MapFrom(src => src.ShvaTransactionDetails.ShvaAuthNum))
         //       .ForMember(q => q.CardDate, src => src.MapFrom(src => src.CreditCardDetails.CardExpiration))
         //       .ForMember(q => q.DealID, src => src.MapFrom(src => src.PaymentTransactionID))
         //       .ForMember(q => q.Terminal, src => src.MapFrom(src => src.ShvaTransactionDetails.ShvaTerminalID))
         //       .ForMember(q => q.DealNumber, src => src.MapFrom(src => src.ShvaTransactionDetails.ShvaDealID))
         //       .ForMember(q => q.DealDate, src => src.MapFrom(src => src.TransactionDate))
         //       .ForMember(q => q.PayNumber, src => src.MapFrom(src => src.NumberOfPayments - 1))//Pay Number is added payments
         //       .ForMember(q => q.FirstPay, src => src.MapFrom(src => src.InitialPaymentAmount))
         //       .ForMember(q => q.AddPay, src => src.MapFrom(src => src.InstallmentPaymentAmount))
         //       .ForMember(q => q.CardNumber, src => src.MapFrom(src => src.CreditCardDetails.CardNumber))
         //       .ForMember(q => q.CardOwner, src => src.MapFrom(src => src.CreditCardDetails.CardOwnerName))
         //       .ForMember(q => q.Manpik, src => src.MapFrom(src => src.CreditCardDetails.CardVendor))
         //   .ForMember(q => q.OriginalUID, src => src.MapFrom(src => src.ShvaTransactionDetails.ShvaDealID))
         //   .ForMember(q => q.CardNameIDCode, src => src.MapFrom(src => (int)src.ShvaTransactionDetails.Solek))
         //   .ForMember(q => q.EmvSoftVersion, src => src.MapFrom(src => src.EmvSoftVersion))
         //   .ForMember(q => q.CompRetailerNum, src => src.MapFrom(src => src.CompRetailerNum))
         //   .ForMember(q => q.CurrencyID, src => src.MapFrom(src => LegacyQueryStringConvertor.GetLegacyCurrencyValue(src.Currency)))
         //    .ForMember(q => q.Currency, src => src.MapFrom(src => LegacyQueryStringConvertor.GetLegacyCurrency(src.Currency)))
         //    .ForMember(q => q.ManpikID, src => src.MapFrom(src => LegacyQueryStringConvertor.GetInValueManpik(src.CreditCardDetails.CardVendor)))
         //   .ForMember(q => q.MutagID, src => src.MapFrom(src => src.CreditCardDetails.CardBrand))
         //   .ForMember(q => q.Mutag, src => src.MapFrom(src => LegacyQueryStringConvertor.GetMutagStr(src.CreditCardDetails.CardBrand)))
         // .ForMember(q => q.DealType, src => src.MapFrom(src => src.TransactionType.ToString()))
         //.ForMember(q => q.DealTypeID, src => src.MapFrom(src => LegacyQueryStringConvertor.GetLegacyDealtypeValue(src.TransactionType)))
         //.ForMember(q => q.Token, src => src.MapFrom(src => src.CreditCardToken));


            //string message = CardVendor.ResourceManager.GetString("NameOfKey", culture);
            ResourceManager rm = new ResourceManager("CardVendor", Assembly.GetExecutingAssembly());
            return new LegacyQueryStringModel
            {
                DealID = paymentTransaction.PaymentTransactionID.ToString(),
                StateData = request.StateData,
                Total = paymentTransaction.TotalAmount.ToString("F2"),
                CardOwner = request.Name,
                OwnerEmail = request.Email,
                Id = "TODO",//request.    redirectpaymentpageid
                //OkNumber = paymentTransaction.OkNumber,
                //Code = paymentTransaction.Code,
                //DealID = paymentTransaction.DealID,
                //BusinessName = terminalDetails.Label,
                //Terminal = paymentTransaction.Terminal,
                //DealNumber = paymentTransaction.DealNumber.Length > 20 ? paymentTransaction.DealNumber.Substring(paymentTransaction.DealNumber.Length - 20, 20) : paymentTransaction.DealNumber,//
                //CardNumber = paymentTransaction.CardNumber,
                //DealDate = paymentTransaction.DealDate,
                //PayNumber = paymentTransaction.PayNumber,
                //FirstPay = paymentTransaction.FirstPay,
                //AddPay = paymentTransaction.AddPay,
                //DealTypeOut = paymentTransaction.DealTypeOut,
                //DealType = DealType.ResourceManager.GetString(paymentTransaction.DealType, culture),
                //Currency = paymentTransaction.Currency,
                //CardNameID = CardVendor.ResourceManager.GetString(paymentTransaction.CardNameID, culture) ,
                //Manpik = CardVendor.ResourceManager.GetString(paymentTransaction.Manpik, culture),
                //Mutag = paymentTransaction.Mutag,
                //DealTypeID = paymentTransaction.DealTypeID,
                //CurrencyID = paymentTransaction.CurrencyID,
                //CardNameIDCode = paymentTransaction.CardNameIDCode,
                //ManpikID = paymentTransaction.ManpikID,
                //MutagID = paymentTransaction.MutagID, 
                //Tz = request.NationalID,
                //CardDate = paymentTransaction.CardDate,
                //Token = paymentTransaction.Token,
                //PhoneNumber = request.Phone,
                //EmvSoftVersion = paymentTransaction.EmvSoftVersion,
                //OriginalUID = paymentTransaction.OriginalUID,
                //CompRetailerNum = paymentTransaction.CompRetailerNum
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
