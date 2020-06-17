using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Api
{
    public class RequestLogStorageService : IRequestLogStorageService
    {
        private readonly CloudTable table;

        public RequestLogStorageService(IOptions<RequestResponseLoggingSettings> configuration)
        {
            var cfg = configuration?.Value;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(cfg.StorageConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            table = tableClient.GetTableReference(cfg.RequestsLogStorageTable);

            table.CreateIfNotExistsAsync().Wait();
        }

        public async Task Save(LogRequestEntity entity)
        {
            TableOperation insertOperation = TableOperation.Insert(entity);

            await table.ExecuteAsync(insertOperation);
        }

        public async Task<LogRequestEntity> Get(DateTime requestDate, string correlationId)
        {
            TableOperation getOperation = TableOperation.Retrieve<LogRequestEntity>(requestDate.ToString("yy-MM-dd"), correlationId);

            var result = await table.ExecuteAsync(getOperation);

            if (result.Result != null)
            {
                return (LogRequestEntity)result.Result;
            }
            else
            {
                return null;
            }
        }
    }
}
