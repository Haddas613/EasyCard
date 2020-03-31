using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Swagger
{
    public class TransactionRequestWithTokenExample : IExamplesProvider<TransactionRequestWithToken>
    {
        public TransactionRequestWithToken GetExamples()
        {
            return new TransactionRequestWithToken() { TransactionAmount = 123.45m, DealDetails = new SharedIntegration.Models.DealDetails { DealDescription = "My deal" } };
        }
    }

    public class TransactionRequestWithCreditCardExample : IExamplesProvider<TransactionRequestWithCreditCard>
    {
        public TransactionRequestWithCreditCard GetExamples()
        {
            return new TransactionRequestWithCreditCard() { TransactionAmount = 123.45m, DealDetails = new SharedIntegration.Models.DealDetails { DealDescription = "My deal" }, CreditCardSecureDetails = new CreditCardSecureDetails { CardNumber = "1234567890" } };
        }
    }
}
