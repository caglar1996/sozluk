using System.Security.Cryptography;
using System.Text;

namespace BlazorSozluk.Common.Infastructure
{
    public class PasswordEncryptor
    {
        public static string Encrpt(string password)
        {
            using var md5 = MD5.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(password);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            return Convert.ToBase64String(hashBytes);
        }
    }
}
