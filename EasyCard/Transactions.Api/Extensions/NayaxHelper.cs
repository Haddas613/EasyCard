using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Extensions
{
    public class NayaxHelper
    {
        public static string GetCardNumber(string maskedPan)
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
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}
