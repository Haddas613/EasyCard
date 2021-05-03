using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace InforU
{
    [XmlRoot(ElementName = "Result")]
    public class SendSmsResponse
    {
        [XmlElement(ElementName = "Status")]
        public string Status { get; set; }

        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }

        [XmlElement(ElementName = "NumberOfRecipients")]
        public string NumberOfRecipients { get; set; }
    }
}
