using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Shared.Helpers.Queue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BasicServices
{
    public class AzureQueue : IQueue
    {
        private CloudQueueClient _client;

        public AzureQueue(string storageConnectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            _client = storageAccount.CreateCloudQueueClient();
        }

        public async Task PushToQueue<T>(string queueName, T model)
        {
            var queue = _client.GetQueueReference(queueName);
            if (!await queue.ExistsAsync())
                throw new StorageException($"Queue '{queueName}' doesn't exists in storage");

            string json = JsonConvert.SerializeObject(model);
            var message = new CloudQueueMessage(json);
            await queue.AddMessageAsync(message);
        }

        public async Task PushToQueue<T>(string queueName, IEnumerable<T> items)
        {
            var queue = _client.GetQueueReference(queueName);
            if (!await queue.ExistsAsync())
                throw new StorageException($"Queue '{queueName}' doesn't exists in storage");

            foreach (var item in items)
            {
                string json = JsonConvert.SerializeObject(item);
                var message = new CloudQueueMessage(json);
                await queue.AddMessageAsync(message);
            }
        }
    }
}
