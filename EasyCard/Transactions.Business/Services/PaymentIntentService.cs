using Azure.Data.Tables;
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
        private readonly TableClient table;

        public PaymentIntentService(string storageConnectionString, string tableName)
        {
            table = new TableClient(storageConnectionString, tableName);

            table.CreateIfNotExists();
        }

        public async Task DeletePaymentIntent(Guid paymentIntentID)
        {
            // TODO: do we need to throw exception here ?

            var paymentIntent = (await table.GetEntityAsync<PaymentIntent>(PartitionKey, paymentIntentID.ToString())).Value;

            if (paymentIntent != null)
            {
                paymentIntent.Deleted = true;

                await table.UpdateEntityAsync(paymentIntent, paymentIntent.ETag);
            }

        }

        public async Task<PaymentRequest> GetPaymentIntent(Guid paymentIntentID, bool showDeleted = false)
        {
            PaymentRequest response = null;

            var paymentIntent = (await table.GetEntityAsync<PaymentIntent>(PartitionKey, paymentIntentID.ToString())).Value;

            if (paymentIntent != null)
            {
                if ((!paymentIntent.Deleted || showDeleted) && !string.IsNullOrWhiteSpace(paymentIntent?.PaymentRequest))
                {
                    response = JsonConvert.DeserializeObject<PaymentRequest>(paymentIntent?.PaymentRequest);
                }
            }

            return response;
        }

        public async Task SavePaymentIntent(PaymentRequest request)
        {
            var entity = new PaymentIntent(request.TerminalID.GetValueOrDefault(), request.PaymentRequestID) { PaymentRequest = JsonConvert.SerializeObject(request) };

            await table.AddEntityAsync(entity);
        }
    }
}
