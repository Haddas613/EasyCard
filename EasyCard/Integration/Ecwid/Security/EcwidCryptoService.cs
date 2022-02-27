using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Ecwid.Security
{
    public class EcwidCryptoService : IEcwidCryptoService
    {
        private readonly int keyLength = 16;
        private readonly int tagLength = AesGcm.TagByteSizes.MaxSize;

        private readonly byte[] secretKeyBytes;

        public EcwidCryptoService(string secretKey)
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

        public string Decrypt(string encryptedText)
        {
            var encryptedTextSpan = Convert.FromBase64String(encryptedText).AsSpan();
            var encryptedData = encryptedTextSpan.Slice(tagLength);

            byte[] iv = encryptedTextSpan.Slice(0, tagLength).ToArray();

            try
            {
                using (var rijndaelManaged = new RijndaelManaged { Key = secretKeyBytes, IV = iv, Mode = CipherMode.CBC })

                using (var memoryStream = new System.IO.MemoryStream(encryptedData.ToArray()))

                using (var cryptoStream =
                       new CryptoStream(
                           memoryStream,
                           rijndaelManaged.CreateDecryptor(secretKeyBytes, iv),
                           CryptoStreamMode.Read))
                {
                    return new System.IO.StreamReader(cryptoStream).ReadToEnd();
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
        }
    }
}
