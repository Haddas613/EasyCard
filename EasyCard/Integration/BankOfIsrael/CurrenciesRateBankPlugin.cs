using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;

namespace BankOfIsrael
{
    public class CurrenciesRateBankPlugin
    {
        private readonly string bankOfIsrael_Xml_Url;

        public CurrenciesRateBankPlugin(string bankOfIsrael_Xml_Url)
        {
            this.bankOfIsrael_Xml_Url = bankOfIsrael_Xml_Url;
        }

        public const string NIS = "₪";

        public ExchangeRates GetRates()
        {
            try
            {
                string xmlStr;

                using (var httpClient = new HttpClient())
                {
                    var xmlUrl = bankOfIsrael_Xml_Url;
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, xmlUrl);

                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36");

                    HttpResponseMessage response = httpClient.SendAsync(request).Result;

                    xmlStr = response.Content.ReadAsStringAsync().Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Error: POST {xmlUrl} {xmlStr} {response.StatusCode}");
                    }
                }

                var resObj = xmlStr.FromXmlString<Currencies>();

                return ConvertRes(resObj);
            }
            catch
            {
                // TODO: convert to application exception
                throw;
            }
        }

        private ExchangeRates ConvertRes(Currencies svcRes)
        {
            var res = new ExchangeRates();

            var date = svcRes.LastUpdate.AddDays(1);

            var rates = new List<ExchangeRate>();

            foreach (var crncyRt in svcRes.CurrenciesList)
            {
                rates.Add(new ExchangeRate
                {
                    Currency = crncyRt.CurrencyCode,
                    Date = date,
                    Rate = crncyRt.Rate
                });
            }

            res.Rates = rates.ToArray();

            return res;

        }

        
    }
}
