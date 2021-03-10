using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger logger;

        public AzureKeyValueStorage(IOptions<AzureKeyVaultSettings> options, ILogger<SecretClient> logger)
        {
            this.secretClient = new SecretClient(
                new Uri(options.Value.KeyVaultUrl),
                    new ClientSecretCredential(options.Value.AzureADApplicationTenant, options.Value.AzureADApplicationId, options.Value.AzureADApplicationSecret));
            this.logger = logger;
        }
        
        public async Task Delete(string key)
        {
            await secretClient.StartDeleteSecretAsync(key);
        }

        public async Task<T> Get(string key)
        {
            try
            {
                var azureResponse = await secretClient.GetSecretAsync(key);

                if (azureResponse?.Value?.Value != null)
                {
                    return JsonConvert.DeserializeObject<T>(azureResponse.Value.Value);
                }
                else
                {
                    return null;
                }
            }
            catch (Azure.RequestFailedException azex)
            {
                logger.LogError(azex, $"Cannot retrieve secret: {key}");
                return null;
            }
        }

        public async Task Save(string key, string value) => _ = await secretClient.SetSecretAsync(new KeyVaultSecret(key, value));
    }
}
