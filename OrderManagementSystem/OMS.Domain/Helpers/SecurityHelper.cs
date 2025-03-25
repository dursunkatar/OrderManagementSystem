using System.Security.Cryptography;
using System.Text;

namespace OMS.Domain.Helpers
{
    public static class SecurityHelper
    {
        public static string CreateMD5Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input), "Hash için girdi boş olamaz.");

            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static bool VerifyMD5Hash(string input, string hash)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(hash))
                return false;

            string hashOfInput = CreateMD5Hash(input);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Compare(hashOfInput, hash) == 0;
        }
    }
}
