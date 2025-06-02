using System;
using System.Security.Cryptography;
using System.Linq;
using System.Text;

namespace SaaS.Helper;

public static class CryptoHelper
{
    public static string Encrypt(string plainText, string key)
    {
        byte[] keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        byte[] result = aes.IV.Concat(cipherBytes).ToArray();
        return Convert.ToBase64String(result);
    }

    public static string Decrypt(string encryptedText, string key)
    {
        byte[] keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        byte[] fullCipher = Convert.FromBase64String(encryptedText);

        using var aes = Aes.Create();
        aes.Key = keyBytes;

        byte[] iv = fullCipher.Take(16).ToArray();
        byte[] cipher = fullCipher.Skip(16).ToArray();
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        byte[] decryptedBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public static string Hash(string value, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        byte[] valueBytes = Encoding.UTF8.GetBytes(value);
        byte[] hashBytes = hmac.ComputeHash(valueBytes);
        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyHash(string value, string expectedHash, string key)
    {
        string computedHash = Hash(value, key);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(expectedHash)
        );
    }

    public static string GenerateGuid()
    {
        return $"{Guid.NewGuid().ToString("N")}{Guid.NewGuid().ToString("N")}";
    }

    public static string GeneratePassword(int length)
    {
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890!@#$%^&*()-=+<,>.";
        char[] password = new char[length];
        byte[] randomBytes = new byte[length];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        for (int i = 0; i < length; i++)
        {
            int index = randomBytes[i] % validChars.Length;
            password[i] = validChars[index];
        }

        return new string(password);
    }

    public static string GenerateRandomCharacterSet(int length)
    {
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
        char[] password = new char[length];
        byte[] randomBytes = new byte[length];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        for (int i = 0; i < length; i++)
        {
            int index = randomBytes[i] % validChars.Length;
            password[i] = validChars[index];
        }

        return new string(password);
    }

}
