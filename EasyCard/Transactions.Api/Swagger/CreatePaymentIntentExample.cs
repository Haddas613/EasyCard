using Newtonsoft.Json.Linq;
using Shared.Helpers;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.PaymentRequests;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Swagger
{
    public class CreatePaymentIntentExample : IExamplesProvider<PaymentRequestCreate>
    {
        public PaymentRequestCreate GetExamples()
        {
            return new PaymentRequestCreate()
            {
                Currency = CurrencyEnum.USD,
                DealDetails = new SharedIntegration.Models.DealDetails
                {
                    DealReference = "123456",
                    ConsumerEmail = "email@example.com",
                    ConsumerPhone = "555-765",
                    DealDescription = "some product pack: 3kg.",
                    ConsumerID = Guid.NewGuid()
                },
                InstallmentDetails = new SharedIntegration.Models.InstallmentDetails
                {
                    InitialPaymentAmount = 200,
                    InstallmentPaymentAmount = 200,
                    NumberOfPayments = 6,
                    TotalAmount = 1200
                },
                PaymentRequestAmount = 1200,
                DueDate = new DateTime(2021, 12, 31),
                RedirectUrl = "https://ecng-testwebstore.azurewebsites.net/PaymentResult?MyOrderID=123456&MySecurityCode=45678912345",
                UserAmount = true,
                Extension = JObject.FromObject(new { CustomPropertyInMySystem = "MyCustomValue" })
            };
        }
    }
}
