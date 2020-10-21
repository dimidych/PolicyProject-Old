using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace PolicyProjectManagementClient
{
    internal static class Hasher
    {
        internal static string Hash(SecureString securePassword)
        {
            var str = ConvertSecureString(securePassword);
            var md5Hasher = MD5.Create();
            var buffer = Encoding.Default.GetBytes(str);
            var data = md5Hasher.ComputeHash(buffer);
            return Bytes2String(data);
        }

        private static string Bytes2String(byte[] data)
        {
            var sBuilder = new StringBuilder();

            foreach (var t in data)
                sBuilder.Append(t.ToString("x2"));

            return sBuilder.ToString();
        }

        private static string ConvertSecureString(SecureString securePassword)
        {
            if (securePassword == null)
                return string.Empty;

            var unmanagedString = IntPtr.Zero;

            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}