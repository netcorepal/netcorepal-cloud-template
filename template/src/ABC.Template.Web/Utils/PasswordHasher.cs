//#if (UseAdmin)
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace ABC.Template.Web.Utils;

public static class PasswordHasher
{
    private static string HashPassword(string value, string salt)
    {
        var valueBytes = KeyDerivation.Pbkdf2(
            password: value,
            salt: Encoding.UTF8.GetBytes(salt),
            prf: KeyDerivationPrf.HMACSHA512,
            iterationCount: 100000,
            numBytesRequested: 256 / 8);

        return Convert.ToBase64String(valueBytes);
    }

    private static string GenerateSalt()
    {
        var randomBytes = new byte[128 / 8];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public static string HashPassword(string password)
    {
        var salt = GenerateSalt();
        var hash = HashPassword(password, salt);
        var result = $"{salt}.{hash}";
        return result;
    }

    public static bool VerifyHashedPassword(string password, string storePassword)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        if (string.IsNullOrEmpty(storePassword))
        {
            throw new ArgumentNullException(nameof(storePassword));
        }

        var parts = storePassword.Split('.');
        var salt = parts[0];
        var hash = parts[1];

        return HashPassword(password, salt) == hash;
    }
}
//#endif
