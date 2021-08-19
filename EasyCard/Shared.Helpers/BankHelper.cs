using Shared.Helpers.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Shared.Api.Models;

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

        public static bool CheckValidAccountDetails(int bankCode, string branchNumber, string AccountNumber)
        {
            bool validAccount = true;
            int branchNumberInt = -1;
            int accountNumber = -1;

            if (string.IsNullOrEmpty(branchNumber) || !int.TryParse(branchNumber, out branchNumberInt))
            {
                return false;
            }

            if (string.IsNullOrEmpty(AccountNumber) || !int.TryParse(AccountNumber, out accountNumber))
            {
                return false;
            }

            int s, t, u = -1;
            int a, b, c, d, e, f, g, h, x = -1;
            if (branchNumber.Length < 3)
            {
                s = 0;
            }
            else
            {
                int.TryParse(branchNumber.Substring(branchNumber.Length - 3, 1), out s);
            }

            if (branchNumber.Length < 2)
            {
                t = 0;
            }
            else
            {
                int.TryParse(branchNumber.Substring(branchNumber.Length - 2, 1), out t);
            }

            int.TryParse(branchNumber.Substring(branchNumber.Length - 1, 1), out u);
            if (AccountNumber.Length < 9)
            {
                a = 0;
            }
            else
            {
                int.TryParse(AccountNumber.Substring(AccountNumber.Length - 9, 1), out a);
            }

            if (AccountNumber.Length < 8)
            {
                b = 0;
            }
            else
            {
                int.TryParse(AccountNumber.Substring(AccountNumber.Length - 8, 1), out b);
            }

            if (AccountNumber.Length < 7)
            {
                c = 0;
            }
            else
            {
                int.TryParse(AccountNumber.Substring(AccountNumber.Length - 7, 1), out c);
            }

            if (AccountNumber.Length < 6)
            {
                d = 0;
            }
            else
            {
                int.TryParse(AccountNumber.Substring(AccountNumber.Length - 6, 1), out d);
            }

            if (AccountNumber.Length < 5)
            {
                e = 0;
            }
            else
            {
                int.TryParse(AccountNumber.Substring(AccountNumber.Length - 5, 1), out e);
            }

            if (AccountNumber.Length < 4)
            {
                f = 0;
            }
            else
            {
                int.TryParse(AccountNumber.Substring(AccountNumber.Length - 4, 1), out f);
            }

            if (AccountNumber.Length < 3)
            {
                g = 0;
            }
            else
            {
                int.TryParse(AccountNumber.Substring(AccountNumber.Length - 3, 1), out g);
            }

            if (AccountNumber.Length < 2)
            {
                h = 0;
            }
            else
            {
                int.TryParse(AccountNumber.Substring(AccountNumber.Length - 2, 1), out h);
            }

            int.TryParse(AccountNumber.Substring(AccountNumber.Length - 1, 1), out x);
            int sum = 0;
            if (bankCode == 12 && branchNumberInt == 595)
            {
                return false;
            }

            switch (bankCode)
            {
                case 10:
                case 13:
                case 34:
                    sum = ((s * 10) + (t * 9) + (u * 8) + (b * 7) + (c * 6) + (d * 5) + (e * 4) + (f * 3) + (g * 2) + (10 * h) + x) % 100;
                    validAccount = sum.IsIn(90, 72, 70, 60, 20);
                    break;
                case 12:
                    sum = ((s * 9) + (t * 8) + (u * 7) + (d * 6) + (e * 5) + (f * 4) + (g * 3) + (2 * h) + x) % 11;
                    validAccount = sum.IsIn(0, 2, 4, 6);
                    break;
                case 4:
                    sum = ((s * 9) + (t * 8) + (u * 7) + (d * 6) + (e * 5) + (f * 4) + (g * 3) + (2 * h) + x) % 11;
                    validAccount = sum.IsIn(0, 2);
                    break;
                case 11:
                case 17:
                    sum = ((9 * a) + (8 * b) + (7 * c) + (6 * d) + (5 * e) + (4 * f) + (3 * g) + (2 * h) + x) % 11;
                    validAccount = sum.IsIn(4, 0, 2);
                    break;
                case 20:
                    if ((s > 4) || (s == 4 && !(t == 0 && u == 0)))
                    {
                        s = s - 4;
                    }

                    sum = ((9 * s) + (8 * t) + (7 * u) + (6 * d) + (5 * e) + (4 * f) + (3 * g) + (2 * h) + x) % 11;
                    validAccount = sum.IsIn(4, 0, 2);
                    break;
                case 31:
                case 52:

                    sum = ((9 * a) + (8 * b) + (7 * c) + (6 * d) + (5 * e) + (4 * f) + (3 * g) + (2 * h) + x) % 11;
                    validAccount =  sum.IsIn(0, 6);
                    if (!validAccount)
                    {
                        sum = ((6 * d) + (5 * e) + (4 * f) + (3 * g) + (2 * h) + x) % 11;
                        validAccount = sum.IsIn(0, 6);
                    }

                    break;
                case 54:
                    validAccount = true;
                    break;
                case 9:
                    sum = ((a * 9) + (b * 8) + (c * 7) + (d * 6) + (e * 5) + (f * 4) + (g * 3) + (h * 2) + (x * 1)) % 10;
                    validAccount = sum == 0;
                    break;
                case 46:
                    sum = ((s * 9) + (t * 8) + (u * 7) + (d * 6) + (e * 5) + (f * 4) + (g * 3) + (h * 2) + (x * 1)) % 11;
                    validAccount = branchNumberInt.IsIn(192, 191, 183, 181, 178, 166, 154, 539, 527, 516, 515, 507, 505, 503) ? sum.IsIn(0, 2) : sum == 0;
                    if (!validAccount)
                    {
                        sum = ((a * 9) + (b * 8) + (c * 7) + (d * 6) + (e * 5) + (f * 4) + (g * 3) + (h * 2) + (x * 1)) % 11;
                        validAccount = sum == 0;

                        if (!validAccount)
                        {
                            sum = ((d * 6) + (e * 5) + (f * 4) + (g * 3) + (h * 2) + (x * 1)) % 11;
                            validAccount = sum == 0;
                        }
                    }

                    break;
                case 14:
                    sum = ((s * 9) + (t * 8) + (u * 7) + (d * 6) + (e * 5) + (f * 4) + (g * 3) + (h * 2) + (x * 1)) % 11;
                    validAccount = branchNumberInt.IsIn(385, 384, 365, 347) ? sum.IsIn(2, 0) : (branchNumberInt.IsIn(363, 362, 361) ? sum.IsIn(4, 2, 0) : sum == 0);

                    if (!validAccount)
                    {
                        sum = ((a * 9) + (b * 8) + (c * 7) + (d * 6) + (e * 5) + (f * 4) + (g * 3) + (h * 2) + (x * 1)) % 11;
                        validAccount = sum == 0;
                        if (!validAccount)
                        {
                            sum = ((d * 6) + (e * 5) + (f * 4) + (g * 3) + (h * 2) + (x * 1)) % 11;
                            validAccount = sum == 0;
                        }
                    }

                    break;
                default:
                    validAccount = false;
                    break;
            }

            return validAccount;


        }
    }
}
