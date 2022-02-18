using Ecwid.Configuration;
using Ecwid.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public EcwidPayload DecryptEcwidPayload(string encryptedRaw)
        {
            var json = decryptor.Decrypt(encryptedRaw);

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
