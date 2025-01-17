﻿using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DesktopEasyCardConvertorECNG.Models.Helper
{
    public static class EnumConvertor
    {
        public static PaymentTypeEnum ConvertToPaymentType(string paymentTypeStr)
        {
            switch (paymentTypeStr)
            {
                case "0":
                    return PaymentTypeEnum.Card;
                case "1":
                    return PaymentTypeEnum.Bank;
                default:
                    return PaymentTypeEnum.Card;
            }
        }
        public static CurrencyEnum ConvertToCurrecy(string currencyDesktopStr)
        {
            switch (currencyDesktopStr)
            {
                case "2": return CurrencyEnum.USD;
                default: return CurrencyEnum.ILS;
            }
        }

        public static Transactions.Shared.Enums.RepeatPeriodTypeEnum ConvertToBillingType(string billingTypeStr)
        {
            switch (billingTypeStr)
            {
                case "1":
                    return Transactions.Shared.Enums.RepeatPeriodTypeEnum.Monthly;
                case "2":
                    return Transactions.Shared.Enums.RepeatPeriodTypeEnum.BiMonthly;
                case "3":
                    return Transactions.Shared.Enums.RepeatPeriodTypeEnum.Quarter;
                case "4":
                    return Transactions.Shared.Enums.RepeatPeriodTypeEnum.Quarter;
                case "12":
                    return Transactions.Shared.Enums.RepeatPeriodTypeEnum.Year;
                case "99":
                    return Transactions.Shared.Enums.RepeatPeriodTypeEnum.OneTime;
                default: return Transactions.Shared.Enums.RepeatPeriodTypeEnum.Monthly;
            }
        }
    }
}
