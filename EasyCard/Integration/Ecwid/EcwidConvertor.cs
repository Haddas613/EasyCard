using Ecwid.Configuration;
using Ecwid.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecwid
{
    public class EcwidConvertor
    {
        private readonly EcwidGlobalSettings config;
        private readonly EcwidDecryptor decryptor;
        private readonly EcwidIntermediateStorage storage;

        public EcwidConvertor(EcwidGlobalSettings config, EcwidIntermediateStorage storage)
        {
            this.config = config;
            this.decryptor = new EcwidDecryptor(config);
            this.storage = storage;
        }

        public async Task<EcwidPayload> DecryptEcwidPayload(string encryptedRaw, string correlationId)
        {
            var json = decryptor.Decrypt(encryptedRaw);

            await storage.StoreIntermediateData(new EcwidIntermediateData(json, correlationId));

            EcwidPayload ecwidModel = null;

            try
            {
                ecwidModel = JsonConvert.DeserializeObject<EcwidPayload>(json);
            }
            catch (Exception ex)
            {
                //TODO: log
                throw;
            }
            
            return ecwidModel;
        }
    }
}
