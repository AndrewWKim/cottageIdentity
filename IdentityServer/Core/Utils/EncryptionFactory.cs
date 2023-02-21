using System.Security.Cryptography;
using System.Text;

namespace IdentityServer.Core.Utils
{
    public static class EncryptionFactory
    {
        public static string HashWithMD5(string value)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] hash = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(value));

                return HashToBinaryString(hash);
            }
        }

        private static string HashToBinaryString(byte[] hash)
        {
            StringBuilder output = new StringBuilder();

            foreach (var t in hash)
            {
                output.Append(t.ToString("x2"));
            }

            return output.ToString();
        }
    }
}
