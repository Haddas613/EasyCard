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

                case TransactionTypeEnum.Immediate:
                    return NayaxCreditTermsEnum.Immediate;

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

        public static int? GetNayaxSapakMutav(this string sapakMutav)
        {
            int SapakMutav = -1;
            Int32.TryParse(sapakMutav, out SapakMutav);
            return SapakMutav > 0 ? SapakMutav :  new Nullable<int>(); 
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
            return phaseResult == PhaseResultEnum.Success;
        }

        public static bool IsSuccessful(this Phase1ResponseBody phase1Result)
        {
            return ((PhaseResultEnum)Convert.ToInt32(phase1Result.statusCode) == PhaseResultEnum.Success || (phase1Result.statusCode == "-999" && phase1Result.statusMessage.Equals("עסקה בתהליך")));
        }

        public static bool IsSuccessful(this PairResponseBody pairResult)
        {
            return (PairResultEnum)Convert.ToInt32(pairResult.statusCode) == PairResultEnum.Success;
        }

        public static bool IsSuccessful(this AuthResponseBody pairResult)
        {
            return (PairResultEnum)Convert.ToInt32(pairResult.statusCode) == PairResultEnum.Success;
        }

        public static TransactionTypeEnum GetTransactionTypeFromNayax(this CreditTermsEnum creditTerm)
        {
            return creditTerm switch
            {
                CreditTermsEnum.credit => TransactionTypeEnum.Credit,
                CreditTermsEnum.immediate => TransactionTypeEnum.Immediate,
                CreditTermsEnum.regular => TransactionTypeEnum.RegularDeal,
                CreditTermsEnum.installments => TransactionTypeEnum.Installments,
                _ => TransactionTypeEnum.RegularDeal,
            };
        }

        public static SolekEnum GetTransactionSolek(this IssuerAquirEnum issuerAquirEnum)
        {
            return issuerAquirEnum switch
            {
                IssuerAquirEnum.Cal => SolekEnum.VISA,
                IssuerAquirEnum.Isracard => SolekEnum.ISRACARD,
                IssuerAquirEnum.LeumiCard => SolekEnum.LEUMI_CARD,
                IssuerAquirEnum.Tourist => SolekEnum.UNKNOWN,
                IssuerAquirEnum.RFU07 => SolekEnum.OTHER,
                _ => SolekEnum.UNKNOWN,
            };
        }

        public static SpecialTransactionTypeEnum GetSpecialTransactionTypeFromNayax(this TranTypeEnum tranType)
        {
            return tranType switch
            {
                TranTypeEnum.refund => SpecialTransactionTypeEnum.Refund,
                TranTypeEnum.charge => SpecialTransactionTypeEnum.RegularDeal,
                _ => SpecialTransactionTypeEnum.RegularDeal,
            };
        }

        public static CurrencyEnum GetCurrencyFromNayax(this CurrencyEnumISO_Code currencyEnum)
        {
            return currencyEnum switch
            {
                CurrencyEnumISO_Code.ILS => CurrencyEnum.ILS,
                CurrencyEnumISO_Code.USD => CurrencyEnum.USD,
                CurrencyEnumISO_Code.EUR => CurrencyEnum.EUR,
                _ => CurrencyEnum.ILS,
            };
        }

        // TODO: should be replaced to regex
        public static string GetCardNumber(this string maskedPan)
        {
            try
            {
                string cardNumber = string.Empty;
                bool start = true;
                for (int i = 0; i < maskedPan.Length; i++)
                {
                    if (start)
                    {
                        if (maskedPan[i].Equals('0'))
                        {
                            continue;
                        }
                        else
                        {
                            start = false;
                            cardNumber = string.Format("{0}{1}", cardNumber, maskedPan[i]);
                        }
                    }
                    else
                    {
                        cardNumber = string.Format("{0}{1}", cardNumber, maskedPan.Substring(i));
                        break;
                    }
                }

                return cardNumber;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
