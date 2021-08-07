using AutoMapper;
using Shared.Integration.Models.PaymentDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;

namespace Transactions.Api.Mapping.ValueResolvers
{
    public class PaymentDetailsTransactionValueResolver : IValueResolver<PaymentTransaction, Invoice, IEnumerable<PaymentDetails>>
    {
        public IEnumerable<PaymentDetails> Resolve(PaymentTransaction source, Invoice destination, IEnumerable<PaymentDetails> destMember, ResolutionContext context)
        {
            var pd = new List<PaymentDetails>();

            if (source.CreditCardDetails != null)
            {
                var ccd = context.Mapper.Map<CreditCardPaymentDetails>(source.CreditCardDetails);
                ccd.ShovarNumber = source.ShvaTransactionDetails?.ShvaShovarNumber;

                pd.Add(ccd);
            }

            return pd;
        }
    }
}
