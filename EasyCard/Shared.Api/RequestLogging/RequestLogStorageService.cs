using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Api
{
    public class RequestLogStorageService : IRequestLogStorageService
    {
        private readonly TableClient table;

        public RequestLogStorageService(IOptions<RequestResponseLoggingSettings> configuration)
        {
            var cfg = configuration?.Value;

            table = new TableClient(cfg.StorageConnectionString, cfg.RequestsLogStorageTable);

            table.CreateIfNotExistsAsync().Wait();
        }

        public async Task Save(LogRequestEntity entity)
        {
            await table.AddEntityAsync(entity);
        }

        public async Task<LogRequestEntity> Get(DateTime requestDate, string correlationId)
        {
            return await table.GetEntityAsync<LogRequestEntity>(requestDate.ToString("yy-MM-dd"), correlationId);
        }
    }
}
