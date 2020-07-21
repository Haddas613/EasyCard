using Shared.Helpers;
using Shared.Integration.Models;
using Shva.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shva.Conveters
{
    public static class MetadataConveters
    {
        public static ShvaCurrencyEnum GetShvaCurrency(this CurrencyEnum currency)
        {
            return (ShvaCurrencyEnum)Enum.Parse(typeof(ShvaCurrencyEnum), currency.ToString());
        }

        public static string GetShvaCurrencyStr(this ShvaCurrencyEnum currency)
        {
            return ((int)currency).ToString();
        }

        public static string GetShvaExpDate(this CardExpiration cardExpiration)
        {
            return $"{cardExpiration.Year:00}{cardExpiration.Month:00}";
        }

        public static string ToShvaDecimalStr(this decimal amount)
        {
            return Convert.ToInt32(Math.Round(amount * 100m, 0)).ToString(); // sum in Agurut
        }

        public static ShvaParamJEnum GetParamJ5(this JDealTypeEnum transactionType)
        {
            switch (transactionType)
            {
                case JDealTypeEnum.J4:
                    return ShvaParamJEnum.MakeDeal;

                case JDealTypeEnum.J2:
                    return ShvaParamJEnum.Check;

                case JDealTypeEnum.J5:
                    return ShvaParamJEnum.J5Deal;

                default:
                    throw new NotSupportedException($"Given transaction type {transactionType} is not supported by Shva");
            }
        }

        public static bool IsSuccessful(this AshEndResultEnum ashEndResult)
        {
            return ashEndResult == AshEndResultEnum.Success || ashEndResult == AshEndResultEnum.SuccessJ5;
        }

        public static bool IsSuccessful(this AshStartResultEnum ashStartResult)
        {
            return ashStartResult == AshStartResultEnum.Success || ashStartResult == AshStartResultEnum.Success2 || ashStartResult == AshStartResultEnum.Success3 || ashStartResult == AshStartResultEnum.Success4 || ashStartResult == AshStartResultEnum.Success4 || ashStartResult == AshStartResultEnum.Success6 || ashStartResult == AshStartResultEnum.Success7 || ashStartResult == AshStartResultEnum.Success8 || ashStartResult == AshStartResultEnum.Success9;
        }

        public static bool IsSuccessForContinue(this AshStartResultEnum ashStartResult)
        {
            return ashStartResult == AshStartResultEnum.Success;
        }

        public static bool IsSuccessful(this AshAuthResultEnum ashAuthResult)
        {
            return ashAuthResult == AshAuthResultEnum.Success;
        }

        public static RejectionReasonEnum GetErrorCode(this AshEndResultEnum ashEndResult)
        {
            return RejectionReasonEnum.Unknown;
        }

        public static ShvaTransactionTypeEnum GetShvaTransactionType(this SpecialTransactionTypeEnum transactionType)
        {
            switch (transactionType)
            {
                case SpecialTransactionTypeEnum.RegularDeal:
                    return ShvaTransactionTypeEnum.RegularDeal;

                case SpecialTransactionTypeEnum.InitialDeal:
                    return ShvaTransactionTypeEnum.InitialDeal;

                case SpecialTransactionTypeEnum.Refund:
                    return ShvaTransactionTypeEnum.Refund;

                default:
                    throw new NotSupportedException($"Given transaction type {transactionType} is not supported by Shva");
            }
        }

        public static string GetShvaTransactionTypeStr(this ShvaTransactionTypeEnum transactionType)
        {
            return ((int)transactionType).ToString("00");
        }

        public static ShvaCreditTermsEnum GetShvaCreditTerms(this TransactionTypeEnum transactionType)
        {
            switch (transactionType)
            {
                case TransactionTypeEnum.RegularDeal:
                    return ShvaCreditTermsEnum.Regular;

                case TransactionTypeEnum.Credit:
                    return ShvaCreditTermsEnum.Credit;

                case TransactionTypeEnum.Installments:
                    return ShvaCreditTermsEnum.Installment;

                default:
                    throw new NotSupportedException($"Given transaction type {transactionType} is not supported by Shva");
            }
        }

        public static string GetShvaCreditTermsStr(this ShvaCreditTermsEnum creditTerms)
        {
            return ((int)creditTerms).ToString();
        }

        public static ShvaCardPresenceEnum GetShvaCardPresence(this CardPresenceEnum cardPresence)
        {
            switch (cardPresence)
            {
                case CardPresenceEnum.Regular:
                    return ShvaCardPresenceEnum.Magnetic;

                case CardPresenceEnum.CardNotPresent:
                    return ShvaCardPresenceEnum.TelephoneDdeal;

                default:
                    throw new NotSupportedException($"Given transaction type {cardPresence} is not supported by Shva");
            }
        }

        public static string GetShvaCardPresenceStr(this ShvaCardPresenceEnum cardPresence)
        {
            return ((int)cardPresence).ToString("00");
        }

        public static DateTime? GetDateFromShvaDateTime(this string shvaDateTimeStr)
        {
            if (string.IsNullOrWhiteSpace(shvaDateTimeStr))
            {
                return null;
            }

            // TODO: timezone
            if (DateTime.TryParseExact(shvaDateTimeStr, "MMddHHmmss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime res))
            {
                return res;
            }

            return null;
        }

        public static string GetShvaDateStr(this DateTime? date)
        {
            if (date == null)
            {
                return null;
            }

            return date.Value.ToString("MMdd");
        }

        public static string GetShvaTimeStr(this DateTime? date)
        {
            if (date == null)
            {
                return null;
            }

            return date.Value.ToString("HHmmss");
        }
    }
}
