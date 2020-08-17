using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Billing
{
    public class CurrencyRateTuple
    {
        public decimal? EURRate { get; set; }

        public decimal? USDRate { get; set; }

        public decimal? Convert(CurrencyEnum srcCurrency, decimal? srcValue, CurrencyEnum targetCurrency)
        {
            if (srcValue.GetValueOrDefault() <= 0 || srcCurrency == targetCurrency)
            {
                return srcValue;
            }

            if (targetCurrency == CurrencyEnum.ILS)
            {
                if (srcCurrency == CurrencyEnum.EUR)
                {
                    return Math.Round((srcValue * EURRate).GetValueOrDefault(), 2, MidpointRounding.AwayFromZero);
                }
                else if (srcCurrency == CurrencyEnum.USD)
                {
                    return Math.Round((srcValue * USDRate).GetValueOrDefault(), 2, MidpointRounding.AwayFromZero);
                }
            }

            if (srcCurrency == CurrencyEnum.ILS)
            {
                if (targetCurrency == CurrencyEnum.EUR && EURRate.GetValueOrDefault() > 0)
                {
                    return Math.Round((srcValue / EURRate).GetValueOrDefault(), 2, MidpointRounding.AwayFromZero);
                }
                else if (targetCurrency == CurrencyEnum.USD && USDRate.GetValueOrDefault() > 0)
                {
                    return Math.Round((srcValue / USDRate).GetValueOrDefault(), 2, MidpointRounding.AwayFromZero);
                }
            }

            if (srcCurrency == CurrencyEnum.EUR && targetCurrency == CurrencyEnum.USD)
            {
                var eURtoUSDCrossRate = USDRate > 0 ? EURRate / USDRate : null;

                return Math.Round((srcValue * eURtoUSDCrossRate).GetValueOrDefault(), 2, MidpointRounding.AwayFromZero);
            }

            if (targetCurrency == CurrencyEnum.EUR && srcCurrency == CurrencyEnum.USD)
            {
                var uSDtoEURCrossRate = EURRate > 0 ? USDRate / EURRate : null;

                return Math.Round((srcValue * uSDtoEURCrossRate).GetValueOrDefault(), 2, MidpointRounding.AwayFromZero);
            }

            return null;
        }
    }
}
