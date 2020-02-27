using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Shva.Models
{
    [XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Envelope
    {
        [XmlElement("Header")]
        public Header Header { get; set; }

        [XmlElement("Body")]
        public Body Body { get; set; }
    }

    [XmlRoot(ElementName = "Header", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Header
    {
        public object Content { get; set; }
    }

    [XmlRoot(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Body
    {
        [XmlElement("GetNewToken", Type = typeof(GetNewTokenRequest), Namespace = "http://tempuri.org/")]
        [XmlElement("GetNewTokenResponse", Type = typeof(GetNewTokenResponse), Namespace = "http://tempuri.org/")]

        [XmlElement("DoDealWithToken", Type = typeof(ShvaPaymentTransactionRequest), Namespace = "http://tempuri.org/")]
        [XmlElement("DoDealWithTokenResponse", Type = typeof(ShvaPaymentTransactionResponse), Namespace = "http://tempuri.org/")]
        public object Content { get; set; }
    }
}
