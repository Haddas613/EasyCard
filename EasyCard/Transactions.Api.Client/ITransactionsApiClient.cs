using Bit.Models;
using Shared.Api.Models;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Transactions.Api.Models.Billing;
using Transactions.Api.Models.Checkout;
using Transactions.Api.Models.Currency;
using Transactions.Api.Models.External.Bit;
using Transactions.Api.Models.External.ThreeDS;
using Transactions.Api.Models.Invoicing;
using Transactions.Api.Models.PaymentRequests;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Models.UpdateParameters;

namespace Transactions.Api.Client
{
    public interface ITransactionsApiClient
    {
        Task<OperationResponse> CreateTransaction(CreateTransactionRequest model);

        Task<InitialBitOperationResponse> InitiateBitTransaction(CreateTransactionRequest model);

        Task<OperationResponse> CaptureBitTransaction(CaptureBitTransactionRequest model);

        Task<OperationResponse> BitTransactionPostProcessing(Guid? transactionID);

        Task<BitTransactionResponse> GetBitTransaction(GetBitTransactionQuery request);

        Task<OperationResponse> CreateTransactionPR(PRCreateTransactionRequest model);

        Task<CheckoutData> GetCheckout(Guid? paymentRequestID, Guid? paymentIntentID, string apiKey);

        Task<OperationResponse> GenerateInvoice(Guid? invoiceID);

        Task<CreateInvoicingConsumerResponse> CreateInvoicingConsumer(CreateInvoicingConsumerRequest consumerRequest);

        Task<OperationResponse> TransmitTerminalTransactions(Guid? terminalID);

        Task<SummariesResponse<TransmitTransactionResponse>> TransmitTransactions(TransmitTransactionsRequest request);

        Task<CreateTransactionFromBillingDealsResponse> CreateTransactionsFromBillingDeals(CreateTransactionFromBillingDealsRequest request);

        Task<IEnumerable<Guid>> GetNonTransmittedTransactionsTerminals();

        Task<SendBillingDealsToQueueResponse> SendBillingDealsToQueue(Guid terminalID);

        Task<OperationResponse> CancelPaymentRequest(Guid paymentRequestID);

        Task<OperationResponse> CancelPaymentIntent(Guid paymentIntentID);

        Task<OperationResponse> CreatePaymentIntent(PaymentRequestCreate model);

        Task<OperationResponse> DeleteConsumerRelatedData(Guid consumerID);

        Task<TransactionResponseAdmin> GetTransaction(Guid? transactionID);

        Task<OperationResponse> GenerateMasavFile();

        Task<OperationResponse> UpdateCurrencyRates(CurrencyRateUpdateRequest request);

        Task<OperationResponse> PauseBillingDeal(Guid billingDealID, PauseBillingDealRequest model);

        Task<SummariesAmountResponse<TransactionSummaryAdmin>> GetTransactions(TransactionsFilter filter);

        Task<Versioning3DsResponse> Versioning3Ds(Versioning3DsRequest request);

        Task<Authenticate3DsResponse> Authenticate3Ds(Authenticate3DsRequest request);

        Task<OperationResponse> CardTokensRetentionCleanup();
    }
}
