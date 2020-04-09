using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions;
using Shared.Helpers;
using Shared.Integration.Models;
using Enums = Transactions.Shared.Enums;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Swagger
{
    public class CreateTransactionRequestExample : IExamplesProvider<CreateTransactionRequest>
    {
        public CreateTransactionRequest GetExamples()
        {
            return new CreateTransactionRequest()
            {
                TerminalID = Guid.NewGuid(),
                TransactionType = TransactionTypeEnum.RegularDeal,
                Currency = CurrencyEnum.USD,
                CardPresence = CardPresenceEnum.CardNotPresent,
                CreditCardSecureDetails = new CreditCardSecureDetails
                {
                    CardExpiration = new CardExpiration { Year = 2021, Month = 8 },
                    CardNumber = "2673466233769269",
                    Cvv = "123",
                    CardOwnerNationalID = "5674639482",
                    CardOwnerName = "John Smith"
                },
                TransactionAmount = 123.45m,
                DealDetails = new SharedIntegration.Models.DealDetails
                {
                    DealReference = "123456",
                    ConsumerEmail = "email@example.com",
                    ConsumerPhone = "555-765",
                    DealDescription = "some product pack: 3kg."
                }
            };
        }
    }
}
