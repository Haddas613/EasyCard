using Azure.Data.Tables;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Transactions.Business.Services
{
    public class ThreeDSIntermediateStorage : IThreeDSIntermediateStorage
    {
        private const string PartitionKey = "1";
        private readonly TableClient table;

        public ThreeDSIntermediateStorage(string storageConnectionString, string tableName)
        {
            table = new TableClient(storageConnectionString, tableName);

            table.CreateIfNotExists();
        }

        public async Task<ThreeDSIntermediateData> GetIntermediateData(string threeDSServerTransID)
        {
            ThreeDSIntermediateData response = null;

            try
            {
                response = (await table.GetEntityAsync<ThreeDSIntermediateData>(PartitionKey, threeDSServerTransID)).Value;
            }
            catch (Azure.RequestFailedException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }

            return response;
        }

        public async Task StoreIntermediateData(ThreeDSIntermediateData data)
        {
            await table.AddEntityAsync(data);
        }
    }
}
