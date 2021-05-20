using ShvaEMV;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Shva.Models
{
    [XmlRoot(ElementName = "Envelope", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public class Envelope
    {
        [XmlElement("Header")]
        public Header Header { get; set; }

        [XmlElement("Body")]
        public Body Body { get; set; }
    }

#pragma warning disable SA1402

    [XmlRoot(ElementName = "Header", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public class Header
    {
        public object Content { get; set; }
    }

    [XmlRoot(ElementName = "Body", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public class Body
    {
        [XmlElement("AshStartResponse", Type = typeof(AshStartResponseBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        [XmlElement("AshStart", Type = typeof(AshStartRequestBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        [XmlElement("AshEndResponse", Type = typeof(AshEndResponseBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        [XmlElement("AshEnd", Type = typeof(AshEndRequestBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        [XmlElement("AshAuthResponse", Type = typeof(AshAuthResponseBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        [XmlElement("AshAuth", Type = typeof(AshAuthRequestBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        [XmlElement("TransEMV", Type = typeof(TransEMVRequestBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        [XmlElement("TransEMVResponse", Type = typeof(TransEMVResponseBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        [XmlElement("ChangePassword", Type = typeof(ChangePasswordRequestBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        [XmlElement("ChangePasswordResponse", Type = typeof(ChangePasswordResponseBody), Namespace = "http://shva.co.il/xmlwebservices/")]
        public object Content { get; set; }
    }

#pragma warning restore SA1402
}
