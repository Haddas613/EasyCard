using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BasicServices
{
    public class IntegrationRequestLogStorageService : IIntegrationRequestLogStorageService
    {
        private readonly CloudTable _table;

        public string StorageTableName => _table?.Name;

        public IntegrationRequestLogStorageService(string storageConnectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            _table = tableClient.GetTableReference(tableName);

            _table.CreateIfNotExistsAsync().Wait();
        }

        public async Task Save(IntegrationMessage entity)
        {
            TableOperation insertOperation = TableOperation.Insert(entity);

            await _table.ExecuteAsync(insertOperation);
        }

        public async Task<IntegrationMessage> Get(DateTime requestDate, string correlationId)
        {
            TableOperation getOperation = TableOperation.Retrieve<IntegrationMessage>(requestDate.ToString("yy-MM-dd"), correlationId);

            var result = await _table.ExecuteAsync(getOperation);

            if (result.Result != null)
            {
                return (IntegrationMessage)result.Result;
            }
            else
            {
                return null;
            }
        }
    }
}
