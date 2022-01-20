using Ecwid.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Api.Models.Transactions;

namespace Ecwid
{
    public class EcwidConvertor
    {
        private readonly EcwidGlobalSettings config;
        private readonly EcwidDecryptor decryptor;

        public EcwidConvertor(EcwidGlobalSettings config)
        {
            this.config = config;
            this.decryptor = new EcwidDecryptor(config);
        }

        /// <summary>
        /// Converts 
        /// </summary>
        /// <param name="encryptedRaw"></param>
        /// <returns></returns>
        public CreateTransactionRequest GetCreateTransactionRequest(string encryptedRaw)
        {
            var json = decryptor.Decrypt(encryptedRaw);

            throw new NotImplementedException();
        }
    }
}
