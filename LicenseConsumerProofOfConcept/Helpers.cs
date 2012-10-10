using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml;

namespace LicenseProofOfConcept
{
    static class Helpers
    {
        public static byte[] ReadBytes(this Stream stream, int count)
        {
            var result = new byte[count];
            stream.Read(result, 0, count);
            return result;
        }

        public  static byte[] ReadToEnd(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static byte[] ToBytes(this XDocument xDoc)
        {
            var memoryStream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
            {
                xDoc.WriteTo(xmlWriter);
            }
            memoryStream.Position = 0;
            return memoryStream.ToArray();
        }
    }
}
