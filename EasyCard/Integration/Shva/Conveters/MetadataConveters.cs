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
            string cardExpMYY = cardExpiration.ToString();//MM/yy
            string[] arrCardExpValues = cardExpMYY.Split('/');
            return string.Format("{0}{1}", arrCardExpValues[1], arrCardExpValues[0]);
        }

        public static string ToShvaDecimalStr(this decimal amount)
        {
            return (amount * 100).ToString(); // sum in Agurut
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

        public static RejectionReasonEnum GetErrorCode(this AshEndResultEnum ashEndResult)
        {
            return RejectionReasonEnum.Unknown;
        }

        // TODO ?
        public static ShvaTransactionTypeEnum GetShvaTransactionType(this TransactionTypeEnum transactionType)
        {
            switch (transactionType)
            {
                case TransactionTypeEnum.RegularDeal:
                    return ShvaTransactionTypeEnum.RegularDeal;

                case TransactionTypeEnum.Credit:
                    return ShvaTransactionTypeEnum.RegularDeal;

                case TransactionTypeEnum.Installments:
                    return ShvaTransactionTypeEnum.RegularDeal;

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
    }
}
