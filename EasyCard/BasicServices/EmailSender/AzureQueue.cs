using Azure.Storage.Queues;
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
        private readonly QueueClient _client;

        public AzureQueue(string storageConnectionString, string queueName)
        {
            _client = new QueueClient(storageConnectionString, queueName);
            
            _client.CreateIfNotExists();
        }

        public async Task PushToQueue<T>(T model)
        {
            string json = JsonConvert.SerializeObject(model);

            await _client.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json)));
        }
    }
}
