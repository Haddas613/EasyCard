using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace BankOfIsrael
{
    public static class SerializationHelper
    {
        public static T FromXmlString<T>(this string str) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var stringReader = new StringReader(str))
            using (var reader = XmlReader.Create(stringReader))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        public static string ToXmlString<T>(this T obj) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter))
            {
                serializer.Serialize(xmlWriter, obj);
                return stringWriter.ToString();
            }
        }
    }
}
