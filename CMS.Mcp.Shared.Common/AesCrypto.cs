namespace CMS.Mcp.Shared.Common
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    public class AesCrypto(string key, string iv)
    {
        private readonly byte[] _keyBytes = Convert.FromBase64String(key);
        private readonly byte[] _ivBytes = Convert.FromBase64String(iv);

        public string Decrypt(string cipherText)
        {
            var cipherBytes = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            var decryptor = aes.CreateDecryptor(_keyBytes, _ivBytes);
            using var memoryStream = new MemoryStream(cipherBytes);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }
    }
}