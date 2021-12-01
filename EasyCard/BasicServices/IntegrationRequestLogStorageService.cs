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
using Azure.Data.Tables;
using System.Linq;

namespace BasicServices
{
    public class IntegrationRequestLogStorageService : IIntegrationRequestLogStorageService
    {
        private readonly TableClient _table;

        public string StorageTableName => _table?.Name;

        private string _containerName;

        private BlobContainerClient _client;

        // TODO: logger
        public IntegrationRequestLogStorageService(string storageConnectionString, string tableName, string containerName)
        {
            _table = new TableClient(storageConnectionString, tableName);

            _table.CreateIfNotExists();

            _containerName = containerName;

            if (_containerName != null)
            {
                _client = new BlobContainerClient(storageConnectionString, _containerName);

                _client.CreateIfNotExists();
            }

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

                await _table.AddEntityAsync(entity);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<IntegrationMessage>> GetAll(string entityID)
        {
            var segment = _table.QueryAsync<IntegrationMessage>($"PartitionKey eq '{entityID}'", 100);

            var e = segment.AsPages().GetAsyncEnumerator();

            if (await e.MoveNextAsync()) 
            { 
                var res = e.Current;

                return res.Values;
            }

            return Enumerable.Empty<IntegrationMessage>();
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
