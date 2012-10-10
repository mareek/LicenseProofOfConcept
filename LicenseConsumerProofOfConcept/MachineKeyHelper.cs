using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Security.Cryptography;

namespace LicenseProofOfConcept
{
    static class MachineKeyHelper
    {
        public static string GetMachineKey()
        {
            using (var sha = SHA256.Create())
            {
                return Convert.ToBase64String(sha.ComputeHash(UTF8Encoding.UTF8.GetBytes(GetMotherBoardSerialNumber()))).Replace("=", "");
            }
        }

        private static string GetMotherBoardSerialNumber()
        {
            return SearcherHelper("Win32_BaseBoard", "SerialNumber").First().ToString();
        }

        private static IEnumerable<object> SearcherHelper(string className, string propertyName)
        {
            var searcher = new ManagementObjectSearcher(string.Format("select {0} from {1}", propertyName, className));

            foreach (ManagementBaseObject managementObject in searcher.Get())
            {
                yield return managementObject[propertyName];
            }
        }
    }
}
