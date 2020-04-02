using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions;
using Shared.Helpers;
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
                TerminalID = "tm_Bqzmv4Wj8UuMTKuPAQ8O4g",
                TransactionType = Shared.Enums.TransactionTypeEnum.RegularDeal,
                Currency = CurrencyEnum.USD,
                CardPresence = Enums.CardPresenceEnum.CardNotPresent,
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
