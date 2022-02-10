using Ecwid.Configuration;
using Ecwid.Security;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecwid
{
    internal class EcwidDecryptor
    {
        private readonly EcwidGlobalSettings config;
        private readonly IEcwidCryptoService ecwidCryptoService;

        public EcwidDecryptor(EcwidGlobalSettings config)
        {
            this.config = config;

            // Get the encryption key (16 first bytes of the app's client_secret key)
            var encryptionKey = config.ClientSecret.Substring(0, 16);
            var cryptoService = new EcwidCryptoService(encryptionKey);

            ecwidCryptoService = cryptoService;
        }

        /// <summary>
        /// Returns Ecwid JSON response
        /// </summary>
        /// <param name="rawEcwidRequest">Raw (url-safe) base64 string from Ecwid request</param>
        /// <returns>JSON string</returns>
        public string Decrypt(string rawEcwidRequest)
        {
            // For correct payload decryption, create additional padding to make the payload a multiple of 2:
            string paddedBase64 = rawEcwidRequest.PadRight(rawEcwidRequest.Length + (rawEcwidRequest.Length % 2), '=');

            // Ecwid sends data in url-safe base64. Convert the raw data to the original base64 first
            string base64Original = paddedBase64.Replace("-", "+").Replace("_", "/");

            return ecwidCryptoService.Decrypt(base64Original);
        }
    }
}
