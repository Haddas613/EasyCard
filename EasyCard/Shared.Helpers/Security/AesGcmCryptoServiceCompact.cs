using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Shared.Helpers.Security
{
    public class AesGcmCryptoServiceCompact : ICryptoServiceCompact
    {
        private readonly int keyLength = 16;
        private readonly int nonceLength = AesGcm.NonceByteSizes.MaxSize;
        private readonly int tagLength = AesGcm.TagByteSizes.MaxSize;

        private readonly byte[] secretKeyBytes;

        public AesGcmCryptoServiceCompact(string secretKey)
        {
            if (secretKey is null)
            {
                throw new ArgumentNullException(nameof(secretKey));
            }

            secretKeyBytes = Encoding.ASCII.GetBytes(secretKey);

            if (secretKeyBytes.Length != keyLength)
            {
                throw new ArgumentException($"Supplied secret key is incorrect. Expected ASCII string with {keyLength} bytes. Got {secretKeyBytes.Length}", nameof(secretKey));
            }
        }

        public string DecryptCompact(string encryptedText)
        {
            var encryptedTextSpan = Convert.FromBase64String(encryptedText).AsSpan();
            var encryptedData = encryptedTextSpan.Slice(nonceLength + tagLength);
            var decryptedData = new byte[encryptedData.Length].AsSpan();
            var nonce = encryptedTextSpan.Slice(0, nonceLength);
            var tag = encryptedTextSpan.Slice(nonceLength, tagLength);

            using var aes = new AesGcm(secretKeyBytes);
            aes.Decrypt(nonce, encryptedData, tag, decryptedData);

            return Encoding.UTF8.GetString(decryptedData);
        }

        public string EncryptCompact(string textToEncrypt)
        {
            var nonce = new byte[nonceLength].AsSpan();
            var tag = new byte[tagLength].AsSpan();
            var plainTextBytes = Encoding.UTF8.GetBytes(textToEncrypt);
            var cipherText = new byte[plainTextBytes.Length].AsSpan();

            RandomNumberGenerator.Fill(nonce);

            using var aes = new AesGcm(secretKeyBytes);
            aes.Encrypt(nonce, plainTextBytes, cipherText, tag);

            var result = new byte[nonce.Length + tag.Length + cipherText.Length].AsSpan();
            nonce.CopyTo(result.Slice(0, nonce.Length));
            tag.CopyTo(result.Slice(nonce.Length, tag.Length));
            cipherText.CopyTo(result.Slice(nonce.Length + tag.Length));

            return Convert.ToBase64String(result);
        }
    }
}
