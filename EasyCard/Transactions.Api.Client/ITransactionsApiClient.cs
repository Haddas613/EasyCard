using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Transactions.Api.Models.Checkout;
using Transactions.Api.Models.Transactions;

namespace Transactions.Api.Client
{
    public interface ITransactionsApiClient
    {
        Task<OperationResponse> CreateTransaction(CreateTransactionRequest model);

        Task<OperationResponse> CreateTransactionPR(PRCreateTransactionRequest model);

        Task<CheckoutData> GetCheckout(Guid? paymentRequestID, string apiKey, Guid? consumerID = null);

        Task<OperationResponse> GenerateInvoice(Guid? invoiceID);

        Task<SummariesResponse<TransmitTransactionResponse>> TransmitTerminalTransactions(Guid? terminalID);

        Task<IEnumerable<Guid>> GetNonTransmittedTransactionsTerminals();

        Task<OperationResponse> CancelPaymentRequest(Guid paymentRequestID);

        Task<OperationResponse> DeleteConsumerRelatedData(Guid consumerID);
    }
}
