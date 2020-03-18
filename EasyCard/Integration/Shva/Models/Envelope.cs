using ShvaEMV;
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
        [XmlElement("AshStartResponse", Type = typeof(AshStartResponseBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        [XmlElement("AshStart", Type = typeof(AshStartRequestBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        [XmlElement("AshEndResponse", Type = typeof(AshEndResponseBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        [XmlElement("AshEnd", Type = typeof(AshEndRequestBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        [XmlElement("AshAuthRespons", Type = typeof(AshAuthResponseBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        [XmlElement("AshAuth", Type = typeof(AshAuthRequestBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        public object Content { get; set; }
    }
}
