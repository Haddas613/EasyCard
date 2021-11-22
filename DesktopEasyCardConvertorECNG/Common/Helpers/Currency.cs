using Common.Models;
using Common.Models.Enums;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Common.Helpers
{
    public static class Currency
    {
        public static double GetApiRate(CurrencyEnum currency)
        {
            try
            {
                string api = "https://www.boi.org.il/currency.xml";
                XmlDocument doc = new XmlDocument();
                doc.Load(api);
                XmlNodeList nodes = doc.DocumentElement.SelectNodes("/CURRENCIES/CURRENCY");

                List<CurrencyItem> currencies = new List<CurrencyItem>();
                int unit = 0;
                foreach (XmlNode node in nodes)
                {
                    CurrencyItem item = new CurrencyItem();
                    item.Name = node.SelectSingleNode("NAME").InnerText;
                    unit = Convert.ToInt32(node.SelectSingleNode("UNIT").InnerText);
                    item.Rate = Convert.ToDouble((Convert.ToDouble(node.SelectSingleNode("RATE").InnerText) / (double)unit).ToString());
                    item.Country = node.SelectSingleNode("COUNTRY").InnerText;
                    if (currency ==  CurrencyEnum.DOLLAR && item.Country == "USA")
                        return item.Rate;
                    else if (currency == CurrencyEnum.EURO && item.Country == "EMU")
                        return item.Rate;
                }
                return 1;
            }
            catch (Exception ex)
            {
               // Logger.Write(ex, "Errors"); Common.Helpers.OtherHelper.SendExcepetionMail(ex);
                return 1;
            }
        }

    }
}
