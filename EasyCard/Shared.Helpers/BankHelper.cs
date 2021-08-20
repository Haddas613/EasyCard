using Shared.Helpers.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers
{
    public static class BankHelper
    {
        public static IEnumerable<BankDetails> GetListOfBankOptions()//TODO TASK ECNG-927
        {
            yield return new BankDetails() { Description = "בחרו בנק מהרשימה", Value = "0" };
            yield return new BankDetails() { Description = "יהב (04)", Value = "04" };
            yield return new BankDetails() { Description = "הדואר (09)", Value = "09" };
            yield return new BankDetails() { Description = "לאומי (10)", Value = "10" };
            yield return new BankDetails() { Description = "דיסקונט (11)", Value = "11" };
            yield return new BankDetails() { Description = "הפועלים (12)", Value = "12" };
            yield return new BankDetails() { Description = "איגוד (13)", Value = "13" };
            yield return new BankDetails() { Description = "אוצר החייל (14)", Value = "14" };
            yield return new BankDetails() { Description = "מרכנתיל דיסקונט (17)", Value = "17" };
            yield return new BankDetails() { Description = "מזרחי (20)", Value = "20" };
            yield return new BankDetails() { Description = "בינלאומי (31)", Value = "31" };
            yield return new BankDetails() { Description = "ערבי ישראלי (34)", Value = "34" };
            yield return new BankDetails() { Description = "מסד (46)", Value = "46" };
            yield return new BankDetails() { Description = "פאגי (52)", Value = "52" };
            yield return new BankDetails() { Description = "ירושלים (54)", Value = "54" };
        }
    }
}
