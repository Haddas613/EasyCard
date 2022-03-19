using Shared.Helpers.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers
{
    public static class CurrencyHelper
    {
        public static string GetCurrencySymbol(this CurrencyEnum currencyEnum)
        {
            return currencyEnum switch
            {
                CurrencyEnum.ILS => "₪",
                CurrencyEnum.USD => "$",
                CurrencyEnum.EUR => "€",
                _ => string.Empty,
            };
        }

        public static int? GetCurrencyISONumber(CurrencyEnum currency)
        {
            return CurrencyDto.GetCurrencyISONumber(currency);
        }
    }
}
