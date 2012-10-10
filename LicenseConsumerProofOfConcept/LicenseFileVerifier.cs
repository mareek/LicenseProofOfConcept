using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Xml;

namespace LicenseProofOfConcept
{
    public static class LicenseFileVerifier
    {
        private const byte currentFileFormatVersion = 0;
        /* File version 0:
         * [0..0] File version
         * [1..4] signature length (sl)
         * [5..sl] signature
         * [sl+1..end] zipped license
         */

        private const string PUBLIC_KEY = @"
<RSAKeyValue>
  <Modulus>sJKuRsD1Uk6c4rtzOGfuhel1sBGY1J0HxEAWROa21c7yy8zPJxvn6mySsCUYamhBEailK4zyz9He/A48F1GV8R2jR7SlG6ppW/O9ZTUeGL74DQTI8EggY+PfTa9xFSH2Bk5UsgqdsNRk1cOGv67WlJoPL9Vn4JkBFJ6gcHAsfds=</Modulus>
  <Exponent>AQAB</Exponent>
</RSAKeyValue>";


        public static bool TryOpenLicenseFile(FileInfo licenseFile, out XDocument xDoc)
        {
            xDoc = null;

            try
            {
                using (var fileStream = licenseFile.OpenRead())
                {
                    var fileVersion = fileStream.ReadByte();
                    var signatureLength = BitConverter.ToInt32(fileStream.ReadBytes(4), 0);
                    var signature = fileStream.ReadBytes(signatureLength);
                    var zippedDoc = fileStream.ReadToEnd();

                    if (fileVersion == currentFileFormatVersion && VerifySignature(zippedDoc, signature))
                    {
                        xDoc = XDocument.Load(new MemoryStream(DecompressZippedData(zippedDoc)));
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private static byte[] DecompressZippedData(byte[] compressedData)
        {
            using (var decompressedStream = new MemoryStream())
            {
                using (var compressedStream = new MemoryStream(compressedData))
                using (var decompressionStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(decompressedStream);
                }
                return decompressedStream.ToArray();
            }
        }

        private static bool VerifySignature(byte[] dataToVerify, byte[] signature)
        {
            using (var rsa = new RSACryptoServiceProvider())
            using (var sha = SHA256.Create())
            {
                rsa.FromXmlString(PUBLIC_KEY);
                return rsa.VerifyHash(sha.ComputeHash(dataToVerify), "SHA256", signature);
            }
        }
    }
}
