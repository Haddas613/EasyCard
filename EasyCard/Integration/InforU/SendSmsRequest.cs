using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace InforU
{
#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable SA1402 // File may only contain a single type
    [XmlRoot(ElementName = "User")]
    public class User
    {
        [XmlElement(ElementName = "Username")]
        public string Username { get; set; }

        [XmlElement(ElementName = "Password")]
        public string Password { get; set; }
    }

    [XmlRoot(ElementName = "Content")]
    public class Content
    {
        [XmlElement(ElementName = "Message")]
        public string Message { get; set; }

        [XmlAttribute(AttributeName = "Type")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "Recipients")]
    public class Recipients
    {
        [XmlElement(ElementName = "PhoneNumber")]
        public string PhoneNumber { get; set; }
    }

    [XmlRoot(ElementName = "Inforu")]
    public class Inforu
    {
        [XmlElement(ElementName = "User")]
        public User User { get; set; }

        [XmlElement(ElementName = "Content")]
        public Content Content { get; set; }

        [XmlElement(ElementName = "Recipients")]
        public Recipients Recipients { get; set; }
    }
#pragma warning restore SA1649 // File name should match first type name
#pragma warning restore SA1402 // File may only contain a single type
}
