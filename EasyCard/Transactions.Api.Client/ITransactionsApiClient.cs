using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Transactions.Api.Models.Billing;
using Transactions.Api.Models.Checkout;
using Transactions.Api.Models.Currency;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Models.UpdateParameters;

namespace Transactions.Api.Client
{
    public interface ITransactionsApiClient
    {
        Task<OperationResponse> CreateTransaction(CreateTransactionRequest model);

        Task<OperationResponse> CreateTransactionPR(PRCreateTransactionRequest model);

        Task<CheckoutData> GetCheckout(Guid? paymentRequestID, Guid? paymentIntentID, string apiKey);

        Task<OperationResponse> GenerateInvoice(Guid? invoiceID);

        Task<SummariesResponse<TransmitTransactionResponse>> TransmitTerminalTransactions(Guid? terminalID);

        Task<CreateTransactionFromBillingDealsResponse> CreateTransactionsFromBillingDeals(CreateTransactionFromBillingDealsRequest request);

        Task<IEnumerable<Guid>> GetNonTransmittedTransactionsTerminals();

        Task<SendBillingDealsToQueueResponse> SendBillingDealsToQueue(Guid terminalID);

        Task<OperationResponse> CancelPaymentRequest(Guid paymentRequestID);

        Task<OperationResponse> DeleteConsumerRelatedData(Guid consumerID);

        Task<TransactionResponseAdmin> GetTransaction(Guid? transactionID);

        Task<OperationResponse> GenerateMasavFile();

        Task<OperationResponse> UpdateCurrencyRates(CurrencyRateUpdateRequest request);

        Task<OperationResponse> PauseBillingDeal(Guid billingDealID, PauseBillingDealRequest model);
    }
}
