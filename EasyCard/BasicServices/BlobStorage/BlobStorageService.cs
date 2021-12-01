using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
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

        public async Task<string> Upload(string filename, Stream stream, string contentType = null)
        {
            var blob = _client.GetBlobClient(filename);
            BlobHttpHeaders blobHeaders = string.IsNullOrWhiteSpace(contentType) ? null : new BlobHttpHeaders { ContentType = contentType };

            try
            {
                await blob.UploadAsync(stream, httpHeaders: blobHeaders);
            }
            catch (Exception e)
            {
                logger.LogError($"{nameof(BlobStorageService)}.{nameof(Upload)} Error: {e.Message}");
                throw;
            }

            return filename;
        }

        public string GetDownloadUrl(string blobUri)
        {
            var blobClient = _client.GetBlobClient(blobUri);

            // Check whether this BlobContainerClient object has been authorized with Shared Key.
            if (blobClient.CanGenerateSasUri)
            {
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = _client.Name,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };

                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                sasBuilder.SetPermissions(BlobSasPermissions.Read |
                    BlobSasPermissions.Write);

                Uri sasUri = blobClient.GenerateSasUri(sasBuilder);

                return sasUri.ToString();
            }
            else
            {
                throw new ApplicationException("BlobContainerClient must be authorized with Shared Key credentials to create a service SAS.");
            }
        }
    }
}
