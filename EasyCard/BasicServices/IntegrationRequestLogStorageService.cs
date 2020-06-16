﻿using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Shared.Helpers;
using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace BasicServices
{
    public class IntegrationRequestLogStorageService : IIntegrationRequestLogStorageService
    {
        private readonly CloudTable _table;

        public string StorageTableName => _table?.Name;

        private CloudBlobClient _blobClient;

        private string _containerName;

        private CloudBlobContainer _container;

        // TODO: logger
        public IntegrationRequestLogStorageService(string storageConnectionString, string tableName, string containerName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            _table = tableClient.GetTableReference(tableName);

            _table.CreateIfNotExistsAsync().Wait();

            _blobClient = storageAccount.CreateCloudBlobClient();

            _containerName = containerName;
        }

        public async Task Save(IntegrationMessage entity)
        {
            try
            {
                if (_containerName != null)
                {
                    if (string.IsNullOrWhiteSpace(entity.Response))
                    {
                        CloudBlockBlob blockBlobResponse = (await GetContainer()).GetBlockBlobReference($"{entity.MessageId}-{entity.Action}-response.xml");

                        blockBlobResponse.Properties.ContentType = "text/xml";

                        await blockBlobResponse.UploadTextAsync(entity.Response);
                    }

                    if (string.IsNullOrWhiteSpace(entity.Request))
                    {
                        CloudBlockBlob blockBlobRequest = (await GetContainer()).GetBlockBlobReference($"{entity.MessageId}-{entity.Action}-request.xml");

                        blockBlobRequest.Properties.ContentType = "text/xml";

                        await blockBlobRequest.UploadTextAsync(entity.Request);
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

        private async Task<CloudBlobContainer> GetContainer()
        {
            if (_container != null && _container.Name == _containerName)
            {
                return _container;
            }

            _container = _blobClient.GetContainerReference(_containerName);

            await _container.CreateIfNotExistsAsync();

            return _container;
        }
    }
}
