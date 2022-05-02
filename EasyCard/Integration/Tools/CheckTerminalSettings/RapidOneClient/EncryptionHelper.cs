using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RapidOne.Helpers
{
    public interface IEncryptionHelper
    {
        string Encrypt(string text);
        string Decrypt(string encryptedText);
        string Base64Encode(string plainText);
        string Base64Decode(string base64EncodedData);
    }

    public class EncryptionHelper: IEncryptionHelper
    {
        private readonly byte[] Key;
        private readonly byte[] IV;

        public EncryptionHelper(byte[] key, byte[] iv)
        {
            this.Key = key;
            this.IV = iv;
        }

        public string Encrypt(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException("textToEncrypt");

            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(
                        msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(encrypted);
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
                throw new ArgumentNullException("encryptedText");

            byte[] cipherText = Convert.FromBase64String(encryptedText);

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(
                        msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;
        }

        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
