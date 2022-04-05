using Newtonsoft.Json.Linq;
using Shared.Helpers;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions;
using SharedHelpers = Shared.Helpers;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Swagger
{
    public class GetTransactionResponseExample : IExamplesProvider<TransactionResponse>
    {
        public TransactionResponse GetExamples()
        {
            return new TransactionResponse
            {
                PaymentTransactionID = Guid.NewGuid(),
                AllowTransmissionCancellation = true,
                TransactionAmount = 1200,
                BillingDealID = null,

                InitialPaymentAmount = 200,
                InstallmentPaymentAmount = 200,
                NumberOfPayments = 6,
                TotalAmount = 1200,

                VATRate = 0.17m,
                VATTotal = 174.36m,
                NetTotal = 1025.64m,
                DocumentOrigin = Shared.Enums.DocumentOriginEnum.Checkout,

                ShvaTransactionDetails = new { ShvaShovarNumber = "123456", ShvaDealID = "12345678901234567890" },
                Currency = SharedHelpers.CurrencyEnum.USD,
                DealDetails = new SharedIntegration.Models.DealDetails
                {
                    DealReference = "123456",
                    ConsumerEmail = "email@example.com",
                    ConsumerPhone = "555-765",
                    DealDescription = "some product pack: 3kg.",
                    ConsumerID = Guid.NewGuid()
                },
                Extension = JObject.FromObject(new { CustomPropertyInMySystem = "MyCustomValue" }),
                CorrelationId = Guid.NewGuid().ToString(),

                TransactionTimestamp = DateTime.UtcNow,
                TransactionDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date,

                Status = Shared.Enums.TransactionStatusEnum.AwaitingForTransmission,

                PaymentTypeEnum = SharedIntegration.Models.PaymentTypeEnum.Card,
                TransactionType = SharedIntegration.Models.TransactionTypeEnum.Installments,
                SpecialTransactionType = SharedIntegration.Models.SpecialTransactionTypeEnum.RegularDeal,
                JDealType = SharedIntegration.Models.JDealTypeEnum.J4,
                CardPresence = SharedIntegration.Models.CardPresenceEnum.CardNotPresent,
                CreditCardDetails = new CreditCardDetails
                {
                    CardNumber = "123456****1234",
                    CardExpiration = new CardExpiration { Year = 2022, Month = 12 },
                    CardOwnerName = "John Smith",
                    CardOwnerNationalID = "1234567",
                    CardBrand = "VISA"
                },
                InvoiceID = Guid.NewGuid(),
                IssueInvoice = true,
                PaymentRequestID = Guid.NewGuid(),
            };
        }
    }
}
