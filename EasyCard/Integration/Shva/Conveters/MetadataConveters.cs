﻿using Shared.Helpers;
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
            // TODO: validate conversion
            return cardExpiration.ToString();
        }

        public static string ToShvaDecimalStr(this decimal amount)
        {
            // TODO: Please describe this conversion ? why we cannot use just round and format ?
            return Math.Round(amount / 100, 2).ToString("00.00").Replace(".", string.Empty);
        }

        public static ShvaParamJEnum GetParamJ5(this TransactionTypeEnum transactionType)
        {
            switch (transactionType)
            {
                case TransactionTypeEnum.RegularDeal:
                case TransactionTypeEnum.Credit:
                case TransactionTypeEnum.FirstInstallment:
                    return ShvaParamJEnum.MakeDeal;

                case TransactionTypeEnum.Check:
                    return ShvaParamJEnum.Check;

                case TransactionTypeEnum.J5Block:
                    return ShvaParamJEnum.J5Deal;

                default:
                    throw new NotSupportedException($"Given transaction type {transactionType} is not supported by Shva");
            }
        }

        public static bool IsSuccessful(this AshEndResultEnum ashEndResult)
        {
            // TODO: implement validation
            return true;
        }

        public static RejectionReasonEnum GetErrorCode(this AshEndResultEnum ashEndResult)
        {
            // TODO: implement conversion
            return RejectionReasonEnum.Unknown;
        }

        public static ShvaTransactionTypeEnum GetShvaTransactionType(this TransactionTypeEnum transactionType)
        {
            switch (transactionType)
            {
                case TransactionTypeEnum.RegularDeal:
                    return ShvaTransactionTypeEnum.RegularDeal;

                case TransactionTypeEnum.Credit:
                    return ShvaTransactionTypeEnum.RegularDeal;

                case TransactionTypeEnum.FirstInstallment:
                    return ShvaTransactionTypeEnum.FirstInstallment;

                case TransactionTypeEnum.Refund:
                    return ShvaTransactionTypeEnum.Refund;

                case TransactionTypeEnum.Check:
                    return ShvaTransactionTypeEnum.RegularDeal;

                case TransactionTypeEnum.J5Block:
                    return ShvaTransactionTypeEnum.RegularDeal;

                default:
                    throw new NotSupportedException($"Given transaction type {transactionType} is not supported by Shva");
            }
        }

        public static string GetShvaTransactionTypeStr(this ShvaTransactionTypeEnum transactionType)
        {
            return ((int)transactionType).ToString("{0:00}");
        }

        public static ShvaCreditTermsEnum GetShvaCreditTerms(this TransactionTypeEnum transactionType)
        {
            switch (transactionType)
            {
                case TransactionTypeEnum.RegularDeal:
                    return ShvaCreditTermsEnum.Regular;

                case TransactionTypeEnum.Credit:
                    return ShvaCreditTermsEnum.Credit;

                case TransactionTypeEnum.FirstInstallment:
                    return ShvaCreditTermsEnum.Installment;

                case TransactionTypeEnum.Refund:
                    return ShvaCreditTermsEnum.Regular;

                case TransactionTypeEnum.Check:
                    return ShvaCreditTermsEnum.Regular;

                case TransactionTypeEnum.J5Block:
                    return ShvaCreditTermsEnum.Regular;

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
            return ((int)cardPresence).ToString("{0:00}");
        }
    }
}