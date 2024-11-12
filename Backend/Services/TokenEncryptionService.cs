namespace Backend.Services;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

public class TokenEncryptionService
{
    private readonly ILogger _logger;
    private readonly string _baseKey;

    public TokenEncryptionService(ILogger<TokenEncryptionService> logger, IConfiguration configuration)
    {
        this._logger = logger;
        this._baseKey = configuration.GetValue<string?>("AppSecret", null) ?? "DEFAULT_SAVEMAIL_KEY";
    }

    private byte[] GenerateKey(int id)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(Encoding.UTF8.GetBytes($"{this._baseKey}{id}"));
    }

    public string Encrypt(string accessToken, int id, string ownerId)
    {
        if (string.IsNullOrEmpty(accessToken)) return accessToken;

        try
        {
            using var aes = Aes.Create();
            aes.Key = this.GenerateKey(id);
            aes.Mode = CipherMode.CBC;
            aes.IV = Encoding.ASCII.GetBytes(ownerId)[..16];

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(accessToken);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        catch
        {
            return accessToken; // Return original on error
        }
    }

    public string Decrypt(string encryptedToken, int id, string ownerId)
    {
        if (string.IsNullOrEmpty(encryptedToken)) return encryptedToken;

        try
        {
            using var aes = Aes.Create();
            aes.Key = this.GenerateKey(id);
            aes.Mode = CipherMode.CBC;
            aes.IV = Encoding.ASCII.GetBytes(ownerId)[..16];

            var cipherBytes = Convert.FromBase64String(encryptedToken);

            using var msDecrypt = new MemoryStream(cipherBytes);
            using var decryptor = aes.CreateDecryptor();
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            
            return srDecrypt.ReadToEnd();
        }
        catch
        {
            return encryptedToken; // Return original on error
        }
    }
}