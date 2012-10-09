using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Text;

namespace LicenseProofOfConcept
{
    public static class LicenseGenerator
    {
        private const byte fileVersion = 0;

        /* File version 0:
         * [0..0] File version
         * [1..4] signature length (sl)
         * [5..sl] signature
         * [sl+1..end] zipped license
         */

        private const string PRIVATE_KEY = @"
<RSAKeyValue>
  <Modulus>sJKuRsD1Uk6c4rtzOGfuhel1sBGY1J0HxEAWROa21c7yy8zPJxvn6mySsCUYamhBEailK4zyz9He/A48F1GV8R2jR7SlG6ppW/O9ZTUeGL74DQTI8EggY+PfTa9xFSH2Bk5UsgqdsNRk1cOGv67WlJoPL9Vn4JkBFJ6gcHAsfds=</Modulus>
  <Exponent>AQAB</Exponent>
  <P>7ZNDkTJzOo2jMTiM11vqHhX9F85S82lOz10Rs3xxzNBR1GSbdcOXOK8tTZWlgsmVn4ErSOXlqDwBI3EBKo+HUw==</P>
  <Q>vkRMhONQrfyB96ftIYL4+Riw7FWl3vO2KmVEEpyJEur5EGgyyofy7dRReqKqAb+K+iP9TsaU22opiAechauGWQ==</Q>
  <DP>f0aTvifO/6F9uhLXsVB2nmOdUbGhUvIp3IG5x/R1awp3rFexyWddjmqa1KPFJcolNGyY6dbwMC7lVT1nKIv4LQ==</DP>
  <DQ>OqXA1GFhGBAyW402idLePaH/vwlzdHK43v6R6g64Lc2h8g28QjN/jRGZ/+wt7RYGl64KQYLylWN248g81fMWGQ==</DQ>
  <InverseQ>pdUQPLG8kEd1GsYdWeQYkQxoNrolZp/RjRSNAGL8vFurTOFd61GbNT8CVOES0uLA7PM8ZxmxcF98bsiXCRTuyQ==</InverseQ>
  <D>hki3M2Xx7AuPMrueL8qS0tKuxx1K3n8h9fVLOlE/wTDm42k6LaMCZ/z0PfOoMtxgh/56xrklvDj+3TAyMQXCAlzHsjhrCTGPAUucYWXrKHIXKiICU5QC5f8j0sJqV2YA5qCmUO2ANpxMYGs0xwbU6qCaAjHHgCZDguKCabvB8TE=</D>
</RSAKeyValue>";

        public static void WriteLicenseFile(FileInfo destFile, XDocument xDoc)
        {
            using (var fileStream = destFile.Create())
            {
                fileStream.WriteByte(fileVersion);

                var signature = SignxDocument(xDoc);
                fileStream.Write(BitConverter.GetBytes(signature.Length), 0, 4);
                fileStream.Write(signature, 0, signature.Length);

                using (var xDocStream = GetStreamFromXdoc(xDoc))
                using (var compressionStream = new DeflateStream(fileStream, CompressionMode.Compress))
                {
                    xDocStream.CopyTo(compressionStream);
                }
            }
        }

        private static Byte[] SignxDocument(XDocument xDoc)
        {
            using (var rsa = new RSACryptoServiceProvider())
            using (var xDocStream = GetStreamFromXdoc(xDoc))
            {
                rsa.FromXmlString(PRIVATE_KEY);
                return rsa.SignData(xDocStream.ToArray(), "SHA256");
            }
        }

        private static MemoryStream GetStreamFromXdoc(XDocument xDoc)
        {
            return new MemoryStream(UTF8Encoding.UTF8.GetBytes(xDoc.ToString()));
        }
    }
}
