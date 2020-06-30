using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;
using Shared.Helpers;
using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;

namespace BasicServices
{
    public class IntegrationRequestLogStorageService : IIntegrationRequestLogStorageService
    {
        private readonly CloudTable _table;

        public string StorageTableName => _table?.Name;

        private string _containerName;

        private BlobContainerClient _client;

        // TODO: logger
        public IntegrationRequestLogStorageService(string storageConnectionString, string tableName, string containerName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            _table = tableClient.GetTableReference(tableName);

            _table.CreateIfNotExists();

            _containerName = containerName;

            _client = new BlobContainerClient(storageConnectionString, _containerName);

            _client.CreateIfNotExists();

        }

        public async Task Save(IntegrationMessage entity)
        {
            try
            {
                if (_containerName != null)
                {
                    if (string.IsNullOrWhiteSpace(entity.Response))
                    {
                        await UploadString($"{entity.MessageId}-{entity.Action}-response.xml", entity.Response);
                    }

                    if (string.IsNullOrWhiteSpace(entity.Request))
                    {
                        await UploadString($"{entity.MessageId}-{entity.Action}-request.xml", entity.Request);
                    }
                }

                // header

                entity.Request = entity.Request?.Left(30000);
                entity.Response = entity.Response?.Left(30000);

                TableOperation insertOperation = TableOperation.Insert(entity);

                await _table.ExecuteAsync(insertOperation);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        // TODO
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

        private async Task UploadString(string fileName, string content)
        {
            var blob = _client.GetBlobClient(fileName);

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(content);
                writer.Flush();
                stream.Position = 0;

                await blob.UploadAsync(stream);
            }

            var headers = new BlobHttpHeaders
            {
                ContentType = "text/xml"
            };

            blob.SetHttpHeaders(headers);
        }
    }
}
