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
using Shared.Helpers.Email;

namespace BasicServices
{
    public class EmailTemplatesStorageService
    {
        private readonly TableClient _table;



        // TODO: logger
        public EmailTemplatesStorageService(string storageConnectionString)
        {
            _table = new TableClient(storageConnectionString, "emailtemplates");

        }

        public async Task Save(EmailTemplateEntity entity)
        {
            try
            {
                await _table.AddEntityAsync(entity);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<EmailTemplateEntity>> GetAll()
        {
            var segment = _table.QueryAsync<EmailTemplateEntity>($"PartitionKey eq '1'", 100);

            var e = segment.AsPages().GetAsyncEnumerator();

            if (await e.MoveNextAsync()) 
            { 
                var res = e.Current;

                return res.Values;
            }

            return Enumerable.Empty<EmailTemplateEntity>();
        }

    }
}
