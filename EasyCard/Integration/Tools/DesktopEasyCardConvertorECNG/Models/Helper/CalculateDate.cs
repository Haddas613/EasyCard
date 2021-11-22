using System;
using System.Collections.Generic;
using System.Text;

namespace DesktopEasyCardConvertorECNG.Models.Helper
{
    public static class CalculateDate
    {
        public static DateTime GetStartPayDate(int payDay, DateTime StartDate)
        {
            payDay = payDay > 28 ? 28 : payDay;
            
            if (StartDate.Day == payDay)
                return StartDate;
            else if (StartDate.Day > payDay)
            {
                return StartDate.AddMonths(1).AddDays(-(StartDate.Day - payDay));
            }
            else//(StartDate.Day < payDay)
            {
                return StartDate.AddDays(payDay - StartDate.Day);
            }
        }
    }
}
