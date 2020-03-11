using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Helpers.KeyValueStorage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BasicServices.KeyValueStorage
{
    public class AzureKeyValueStorage<T> : IKeyValueStorage<T>
        where T : class
    {
        private readonly SecretClient secretClient;

        public AzureKeyValueStorage(IOptions<AzureKeyVaultSettings> options)
        {
            this.secretClient = new SecretClient(
                new Uri(options.Value.KeyVaultUrl),
                    new ClientSecretCredential(options.Value.AzureADApplicationTenant, options.Value.AzureADApplicationId, options.Value.AzureADApplicationSecret));
        }
        
        public async Task Delete(string key) => throw new NotImplementedException();

        public async Task<T> Get(string key)
        {
            var azureResponse = await secretClient.GetSecretAsync(key);

            if (azureResponse?.Value?.Value != null)
            {
                return JsonConvert.DeserializeObject<T>(azureResponse.Value.Value);
            }

            return default(T);
        }

        public async Task Save(string key, string value) => _ = await secretClient.SetSecretAsync(new KeyVaultSecret(key, value));
    }
}
