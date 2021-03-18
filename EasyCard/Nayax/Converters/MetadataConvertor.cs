using Nayax.Models;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nayax.Converters
{
    public static class MetadataConvertor
    {
        public static NayaxTransactionTypeEnum GetNayaxTransactionType(this SpecialTransactionTypeEnum transactionType)
        {
            switch (transactionType)
            {
                case SpecialTransactionTypeEnum.RegularDeal:
                    return NayaxTransactionTypeEnum.RegularDeal;

                case SpecialTransactionTypeEnum.Refund:
                    return NayaxTransactionTypeEnum.Refund;

                default:
                    throw new NotSupportedException($"Given transaction type {transactionType} is not supported by Nayax");
            }
        }

        public static int GetNayaxTransactionType(this NayaxTransactionTypeEnum transactionType)
        {
            return ((int)transactionType);
        }
        public static NayaxCreditTermsEnum GetNayaxCreditTerms(this TransactionTypeEnum transactionType)
        {
            switch (transactionType)
            {
                case TransactionTypeEnum.RegularDeal:
                    return NayaxCreditTermsEnum.Regular;

                case TransactionTypeEnum.Credit:
                    return NayaxCreditTermsEnum.Credit;

                case TransactionTypeEnum.Installments:
                    return NayaxCreditTermsEnum.Payments;

                default:
                    throw new NotSupportedException($"Given transaction type {transactionType} is not supported by Shva");
            }
        }

        public static int GetNayaxCreditTerms(this NayaxCreditTermsEnum creditTerms)
        {
            return ((int)creditTerms);
        }
        public static NayaxCurrencyEnum GetNayaxCurrency(this CurrencyEnum currency)
        {
            return (NayaxCurrencyEnum)Enum.Parse(typeof(NayaxCurrencyEnum), currency.ToString());
        }

        public static string GetNayaxCurrencyStr(this NayaxCurrencyEnum currency)
        {
            return ((int)currency).ToString();
        }

        public static int ToNayaxDecimal(this decimal amount)
        {
            return Convert.ToInt32(Math.Round(amount * 100m, 0)); // sum in Agurut
        }

        public static bool IsSuccessful(this PhaseResultEnum phaseResult)
        {
            return phaseResult ==  PhaseResultEnum.Success;
        }
    }
}
