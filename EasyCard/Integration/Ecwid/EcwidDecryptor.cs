using Ecwid.Configuration;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecwid
{
    internal class EcwidDecryptor
    {
        private readonly EcwidGlobalSettings config;
        private readonly ICryptoServiceCompact cryptoServiceCompact;

        public EcwidDecryptor(EcwidGlobalSettings config)
        {
            this.config = config;

            // Get the encryption key (16 first bytes of the app's client_secret key)
            var encryptionKey = config.ClientSecret.Substring(0, 16);
            var aesGcmService = new AesGcmCryptoServiceCompact(encryptionKey);

            this.cryptoServiceCompact = aesGcmService;
        }

        /// <summary>
        /// Returns Ecwid JSON response
        /// </summary>
        /// <param name="rawEcwidRequest">Raw (url-safe) base64 string from Ecwid request</param>
        /// <returns>JSON string</returns>
        public string Decrypt(string rawEcwidRequest)
        {
            // For correct payload decryption, create additional padding to make the payload a multiple of 4:
            string paddedBase64 = rawEcwidRequest.PadRight(rawEcwidRequest.Length + (4 - (rawEcwidRequest.Length % 4)), '=');

            // Ecwid sends data in url-safe base64. Convert the raw data to the original base64 first
            string base64Original = paddedBase64.Replace('-', '_').Replace('+', '/');

            return cryptoServiceCompact.DecryptCompact(base64Original);
        }
    }
}
