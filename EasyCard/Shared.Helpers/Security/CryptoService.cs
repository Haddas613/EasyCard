using System;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Shared.Helpers.Security
{
    public class CryptoService : ICryptoService
    {
        private X509Certificate2 certtificate;

        public CryptoService(X509Certificate2 certtificate)
        {
            if (certtificate == null)
            {
                throw new ArgumentNullException(nameof(certtificate));
            }

            if (certtificate.GetRSAPublicKey() == null)
            {
                throw new ArgumentException("RSA certificate required", nameof(certtificate));
            }

            this.certtificate = certtificate;
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
            {
                throw new ArgumentNullException(nameof(encryptedText));
            }

            try
            {
                RSA privateKey = (RSA)certtificate.PrivateKey;
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] decryptedBytes = privateKey.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
                string decryptedText = Encoding.UTF8.GetString(decryptedBytes);
                return decryptedText;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to decrypt token: {ex.Message}");
                return null;
            }
        }

        public string Encrypt(string textToEncrypt)
        {
            if (string.IsNullOrWhiteSpace(textToEncrypt))
            {
                throw new ArgumentNullException(nameof(textToEncrypt));
            }

            RSA publicKey = (RSA)this.certtificate.PublicKey.Key;
            byte[] plainBytes = Encoding.UTF8.GetBytes(textToEncrypt);
            byte[] encryptedBytes = publicKey.Encrypt(plainBytes, RSAEncryptionPadding.Pkcs1);
            string encryptedText = Convert.ToBase64String(encryptedBytes);
            return encryptedText;
        }

        public string EncryptWithExpiration(string textToEncrypt, TimeSpan expiredIn)
        {
            if (string.IsNullOrWhiteSpace(textToEncrypt))
            {
                throw new ArgumentNullException(nameof(textToEncrypt));
            }

            if (expiredIn == null)
            {
                throw new ArgumentNullException(nameof(expiredIn));
            }

            var dateStr = DateTime.UtcNow.Add(expiredIn).ToString("o");

            var str = $"{dateStr}|{textToEncrypt}";

            return this.Encrypt(str);
        }

        public string DecryptWithExpiration(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
            {
                throw new ArgumentNullException(nameof(encryptedText));
            }

            var str = this.Decrypt(encryptedText);

            if (str == null)
            {
                return null;
            }

            var parsed = str.Split("|", StringSplitOptions.RemoveEmptyEntries);

            if (parsed.Length != 2)
            {
                return null;
            }

            var dateRes = DateTime.TryParseExact(parsed[0], "o", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime date);

            if (!dateRes)
            {
                return null;
            }

            if (date < DateTime.UtcNow)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(parsed[1]))
            {
                return null;
            }

            return parsed[1];
        }
    }
}
