using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DesktopEasyCardConvertorECNG.Models.Helper
{
   public  class ConvertCardDateToMonthYearcs
    {
         public static CardExpiration GetMonthYearFromCardDate(string cardDate)
        {
            CardExpiration cardExpiration = new CardExpiration();
            if (!(string.IsNullOrEmpty(cardDate) || cardDate.IndexOf('/') == -1 || cardDate.Split('/').Length != 2))
            {
                string monthStr = cardDate.Split('/')[0];
                string yearStr = cardDate.Split('/')[1];
                int month = -1;
                int year = -1;
                Int32.TryParse( monthStr, out month);
                Int32.TryParse(yearStr, out year);
                cardExpiration.Month = month;
                cardExpiration.Year = year;
            }

            return cardExpiration;
        }
    }
}
