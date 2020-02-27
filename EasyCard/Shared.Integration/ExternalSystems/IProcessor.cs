using Shared.Integration.Models;
using System;
using System.Threading.Tasks;

namespace Shared.Integration.ExternalSystems
{
    public interface IProcessor
    {
        Task<ExternalPaymentTransactionResponse> CreateTransaction(ExternalPaymentTransactionRequest paymentTransactionRequest, string messageId, string
             correlationId, Func<IntegrationMessage, IntegrationMessage> handleIntegrationMessage = null);
    }
}
