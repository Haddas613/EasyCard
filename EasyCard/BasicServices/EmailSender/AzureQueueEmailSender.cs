using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Helpers.Email;
using Shared.Helpers.Queue;
using System;
using System.Text;
using System.Threading.Tasks;

namespace BasicServices
{
    /// <summary>
    /// EmailSender based on Azure Queue
    /// </summary>
    /// <remarks>
    /// Note: use as singleton
    /// </remarks>
    public class AzureQueueEmailSender : IEmailSender
    {
        private readonly IQueue queue;
        private readonly TableClient table;

        public AzureQueueEmailSender(IQueue queue, string storageConnectionString, string tableName)
        {
            this.queue = queue;

            table = new TableClient(storageConnectionString, tableName);

            table.CreateIfNotExists();
        }

        public async Task SendEmail(Email email)
        {
            var queueMessage = new EmailQueueMessage(DateTime.UtcNow, Guid.NewGuid());

            var entity = new EmailEntity(queueMessage, email);

            await table.AddEntityAsync(entity);

            await queue.PushToQueue(queueMessage);
        }
    }
}
