using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BasicServices.BlobStorage
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly ILogger logger;

        private string _containerName;
        private BlobContainerClient _client;

        public BlobStorageService(string storageConnectionString, string containerName, ILogger logger)
        {
            this.logger = logger;

            _containerName = containerName;

            if (_containerName != null)
            {
                _client = new BlobContainerClient(storageConnectionString, _containerName);

                _client.CreateIfNotExists();
            }
        }

        public async Task<string> Upload(string filename, Stream stream)
        {
            var blob = _client.GetBlobClient(filename);

            try
            {
                await blob.UploadAsync(stream, true);
            }
            catch(Exception e)
            {
                logger.LogError($"{nameof(BlobStorageService)}.{nameof(Upload)} Error: {e.Message}");
                throw;
            }

            return blob.Uri.ToString();
        }

        public async Task<string> UploadString(string filename, string content)
        {
            var blob = _client.GetBlobClient(filename);

            try
            {
                await blob.UploadAsync(stream, true);
            }
            catch (Exception e)
            {
                logger.LogError($"{nameof(BlobStorageService)}.{nameof(Upload)} Error: {e.Message}");
                throw;
            }

            return blob.Uri.ToString();
        }
    }
}
