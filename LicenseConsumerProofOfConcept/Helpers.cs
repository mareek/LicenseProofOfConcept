using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.IO.Compression;

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

        public static byte[] ReadToEnd(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static void WriteBytes(this Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
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

        public static void CompressToFile(this byte[] data, FileInfo destFile)
        {
            using (var fileStream = destFile.Create())
            using (var compressionStream = new DeflateStream(fileStream, CompressionMode.Compress))
            {
                compressionStream.WriteBytes(data);
            }
        }

        public static byte[] Decompress(this FileInfo destFile)
        {
            using (var fileStream = destFile.OpenRead())
            using (var compressionStream = new DeflateStream(fileStream, CompressionMode.Decompress))
            {
                return compressionStream.ReadToEnd();
            }
        }

    }
}
