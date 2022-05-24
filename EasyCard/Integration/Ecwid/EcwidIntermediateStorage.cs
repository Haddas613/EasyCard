using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ecwid
{
    public class EcwidIntermediateStorage
    {
        private const string PartitionKey = "1";
        private readonly TableClient table;

        public EcwidIntermediateStorage(string storageConnectionString, string tableName)
        {
            table = new TableClient(storageConnectionString, tableName);

            table.CreateIfNotExists();
        }

        public async Task<EcwidIntermediateData> GetIntermediateData(string threeDSServerTransID)
        {
            EcwidIntermediateData response = null;

            try
            {
                response = (await table.GetEntityAsync<EcwidIntermediateData>(PartitionKey, threeDSServerTransID)).Value;
            }
            catch (Azure.RequestFailedException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }

            return response;
        }

        public async Task StoreIntermediateData(EcwidIntermediateData data)
        {
            await table.AddEntityAsync(data);
        }
    }
}
