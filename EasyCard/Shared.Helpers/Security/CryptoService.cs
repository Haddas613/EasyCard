using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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

        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 128;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        private const string passPhrase = "1234567890";

        public string EncryptCompact(string textToEncrypt)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(textToEncrypt);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[16]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }

        public string DecryptCompact(string encryptedText)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(encryptedText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
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
