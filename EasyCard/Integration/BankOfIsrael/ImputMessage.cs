using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BankOfIsrael
{
    [XmlRoot("CURRENCY")]
    public class Currency
    {
        public DateTime LastUpdate { get; set; }

        [XmlElement(ElementName = "NAME")]
        public string Name { get; set; }

        [XmlElement(ElementName = "UNIT")]
        public int Unit { get; set; }

        [XmlElement(ElementName = "CURRENCYCODE")]
        public string CurrencyCode { get; set; }

        [XmlElement(ElementName = "COUNTRY")]
        public string Country { get; set; }

        [XmlElement(ElementName = "RATE")]
        public decimal Rate { get; set; }

        [XmlElement(ElementName = "CHANGE")]
        public double Change { get; set; }
    }

    [XmlRoot("CURRENCIES")]
    public class Currencies
    {
        [XmlElement("LAST_UPDATE")]
        public DateTime LastUpdate { get; set; }
        [XmlElement("CURRENCY")]
        public List<Currency> CurrenciesList { get; set; }
    }
}
