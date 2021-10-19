using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public class PaymentIntentService : IPaymentIntentService
    {
        private const string PartitionKey = "1";
        private readonly CloudTable table;

        public PaymentIntentService(string storageConnectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            table = tableClient.GetTableReference(tableName);

            table.CreateIfNotExists();
        }

        public async Task DeletePaymentIntent(Guid paymentIntentID)
        {
            TableOperation getOperation = TableOperation.Retrieve<PaymentIntent>(PartitionKey, paymentIntentID.ToString());

            var res = await table.ExecuteAsync(getOperation);

            var paymentIntent = res.Result as PaymentIntent;

            if (paymentIntent == null)
            {
                throw new EntityNotFoundException("Payment Intent not found", "PaymentIntent", paymentIntentID.ToString());
            }

            paymentIntent.Deleted = true;

            TableOperation updateOperation = TableOperation.Replace(paymentIntent);
            var resUpdate = await table.ExecuteAsync(updateOperation);

            // TODO: do we need to throw exception here ?
        }

        public async Task<PaymentRequest> GetPaymentIntent(Guid paymentIntentID)
        {
            TableOperation getOperation = TableOperation.Retrieve<PaymentIntent>(PartitionKey, paymentIntentID.ToString());

            PaymentRequest response = null;

            var res = await table.ExecuteAsync(getOperation);

            if (res.Result != null)
            {
                var paymentIntent = (PaymentIntent)res.Result;

                if (!paymentIntent.Deleted && !string.IsNullOrWhiteSpace(paymentIntent?.PaymentRequest))
                {
                    response = JsonConvert.DeserializeObject<PaymentRequest>(paymentIntent?.PaymentRequest);
                }
            }

            return response;
        }

        public async Task SavePaymentIntent(PaymentRequest request)
        {
            var entity = new PaymentIntent(request.TerminalID.GetValueOrDefault(), request.PaymentRequestID) { PaymentRequest = JsonConvert.SerializeObject(request) };

            TableOperation insertOperation = TableOperation.Insert(entity);

            await table.ExecuteAsync(insertOperation);
        }
    }
}
