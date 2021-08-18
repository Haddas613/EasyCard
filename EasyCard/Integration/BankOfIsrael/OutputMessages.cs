using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BankOfIsrael
{
    [XmlRoot("EXCHANGE_RATES")]
    public class ExchangeRates
    {
        [XmlElement("EXCHANGE_RATE")]
        public ExchangeRate[] Rates { get; set; }
    }

    [XmlRoot("EXCHANGE_RATE")]
    public class ExchangeRate
    {
        [XmlElement("CURRENCY")]
        public string Currency { get; set; }

        [XmlElement("RATE")]
        public decimal Rate { get; set; }

        [XmlElement("DATE")]
        public DateTime Date { get; set; }
    }
}
