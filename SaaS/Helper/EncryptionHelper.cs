using System;
using System.Text.Json;

namespace SaaS.Helper;

public class EncryptionHelper
{
    public static string Encrypt<T>(T model)
    {
        string jsonString = JsonSerializer.Serialize(model);
        string? encryptionKey = Environment.GetEnvironmentVariable("EncryptionKey");

        if (string.IsNullOrWhiteSpace(encryptionKey)) throw new Exception("Encryption key is not found in this envrionment");

        string encrypted = CryptoHelper.Encrypt(jsonString, encryptionKey);
        return encrypted;
    }
    public static string Encrypt(string jsonString)
    {
        string? encryptionKey = Environment.GetEnvironmentVariable("EncryptionKey");

        if (string.IsNullOrWhiteSpace(encryptionKey)) throw new Exception("Encryption key is not found in this envrionment");

        string encrypted = CryptoHelper.Encrypt(jsonString, encryptionKey);
        return encrypted;
    }

    public static T Decrypt<T>(string encrypted)
    {
        string? encryptionKey = Environment.GetEnvironmentVariable("EncryptionKey");

        if (string.IsNullOrWhiteSpace(encryptionKey)) throw new Exception("Encryption key is not found in this envrionment");

        string jsonString = CryptoHelper.Decrypt(encrypted, encryptionKey);
        
        T model = JsonSerializer.Deserialize<T>(jsonString) ?? throw new Exception("Invalid Json Data");
        return model;
    }

    public static string Decrypt(string encrypted)
    {
        string? encryptionKey = Environment.GetEnvironmentVariable("EncryptionKey");

        if (string.IsNullOrWhiteSpace(encryptionKey)) throw new Exception("Encryption key is not found in this envrionment");

        string jsonString = CryptoHelper.Decrypt(encrypted, encryptionKey);
        return jsonString;
    }
    public static string Hash(string value)
    {
        string? encryptionKey = Environment.GetEnvironmentVariable("EncryptionKey");
        if (string.IsNullOrWhiteSpace(encryptionKey)) throw new Exception("Encryption key is not found in this envrionment");
        
        string hashed = CryptoHelper.Hash(value, encryptionKey);
        return hashed;
    }

    public static bool VerifyHash(string value, string expectedHash)
    {
        string? encryptionKey = Environment.GetEnvironmentVariable("EncryptionKey");
        if (string.IsNullOrWhiteSpace(encryptionKey)) throw new Exception("Encryption key is not found in this envrionment");
        
        bool hashVerified = CryptoHelper.VerifyHash(value, expectedHash, encryptionKey);
        return hashVerified;
    }

}
