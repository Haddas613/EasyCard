using Shared.Helpers;
using Swashbuckle.AspNetCore.Filters;
using System;
using Transactions.Api.Models.Billing;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Swagger
{
    public class CreateBillingDealExample : IExamplesProvider<BillingDealRequest>
    {
        public BillingDealRequest GetExamples()
        {
            return new BillingDealRequest()
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
                TransactionAmount = 1200,
                CreditCardToken = Guid.NewGuid(),
                BillingSchedule = new Transactions.Shared.Models.BillingSchedule
                {
                    StartAt = DateTime.Now,
                    StartAtType = Transactions.Shared.Enums.StartAtTypeEnum.SpecifiedDate,
                    EndAtType = Transactions.Shared.Enums.EndAtTypeEnum.AfterNumberOfPayments,
                    RepeatPeriodType = Transactions.Shared.Enums.RepeatPeriodTypeEnum.Monthly,
                    EndAtNumberOfPayments = 10
                }
            };
        }
    }
}
