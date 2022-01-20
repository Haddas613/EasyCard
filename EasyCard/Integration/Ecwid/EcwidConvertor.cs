using Ecwid.Configuration;
using Ecwid.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Api.Models.PaymentRequests;
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

        public EcwidOrder DecryptEcwidOrder(string encryptedRaw)
        {
            var json = decryptor.Decrypt(encryptedRaw);

            EcwidOrder ecwidModel = null;

            try
            {
                ecwidModel = JsonConvert.DeserializeObject<EcwidOrder>(json);
            }
            catch (Exception ex)
            {
                //TODO: log
                throw;
            }
            
            return ecwidModel;
        }

        /// <summary>
        /// Converts 
        /// </summary>
        /// <param name="encryptedRaw"></param>
        /// <returns></returns>
        public PaymentRequestCreate GetCreatePaymentRequest(EcwidOrder ecwidOrder)
        {

            var transaction = new PaymentRequestCreate
            {
                Currency = ecwidOrder.Cart.Currency,

                PaymentRequestAmount = ecwidOrder.Total,
                VATTotal = ecwidOrder.Tax,
                NetTotal = ecwidOrder.Total - ecwidOrder.Tax,
                VATRate = ecwidOrder.CustomerTaxExempt ? 0m : (ecwidOrder.Total / ecwidOrder.Tax) - 1,

                CardOwnerNationalID = ecwidOrder.CustomerTaxId,
            };

            return transaction;
        }
    }
}
