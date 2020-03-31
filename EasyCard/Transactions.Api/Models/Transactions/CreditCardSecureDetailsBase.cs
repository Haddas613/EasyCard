using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    [KnownType(typeof(CreditCardSecureDetails))]
    public abstract class CreditCardSecureDetailsBase
    {
    }
}
