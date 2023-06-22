using System.Security.Cryptography;
using System.Text;

namespace ChatService.Extensions;

public static class HashExtension
{
    public static string HashSHA256(this string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }

    public static string BCryptHash(this string input)
    {
        return BCrypt.Net.BCrypt.HashPassword(input);
    }
}
