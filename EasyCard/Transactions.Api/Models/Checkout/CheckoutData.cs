using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.PaymentRequests;

namespace Transactions.Api.Models.Checkout
{
    public class CheckoutData
    {
        // TODO: field whicj indicates that PR already completed or rejected

        public PaymentRequestInfo PaymentRequest { get; set; }

        public TerminalCheckoutCombinedSettings Settings { get; set; }

        public ConsumerInfo Consumer { get; set; }
    }
}
