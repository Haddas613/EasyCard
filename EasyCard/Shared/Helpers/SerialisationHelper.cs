using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Shared.Helpers
{
    public static class SerialisationHelper
    {
        public static string ToXml(this object graph)
        {
            if (graph == null)
            {
                return null;
            }

            XmlSerializer ser = new XmlSerializer(graph.GetType());
            StringBuilder sb = new StringBuilder();

            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
            };

            StringWriter writer = null;
            try
            {
                writer = new StringWriter(sb);
                using (var xmlwriter = XmlWriter.Create(writer, settings))
                {
                    ser.Serialize(xmlwriter, graph);
                }
            }
            finally
            {
                if (writer != null)
                {
                    writer.Dispose();
                }
            }

            return sb.ToString();
        }

        public static T FromXml<T>(this string xml)
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                object obj = null;

                using (StringReader reader = new StringReader(xml))
                {
                    obj = ser.Deserialize(reader);
                }

                return (T)obj;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
