using Shared.Business.Messages;
using Shared.Helpers;
using Shared.Helpers.Models;
using Shared.Integration.Models.PaymentDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Invoicing;
using Transactions.Shared;
using SharedHelpers = Shared.Helpers;
using SharedIntegration = Shared.Integration;
using SharedInvoicing = Shared.Integration.Models.Invoicing;

namespace Transactions.Api.Validation
{
    public class InvoiceValidator
    {
        public static void ValidateInvoiceRequest(InvoiceRequest model)
        {
            List<Error> errors = new List<Error>();

            if (string.IsNullOrWhiteSpace(model.DealDetails?.ConsumerName))
            {
                errors.Add(new Error(nameof(model.DealDetails.ConsumerName), ApiMessages.Required));
            }

            if (string.IsNullOrWhiteSpace(model.DealDetails.ConsumerEmail))
            {
                errors.Add(new Error(nameof(model.DealDetails.ConsumerEmail), ApiMessages.Required));
            }

            if (model.InvoiceDetails.InvoiceType == SharedInvoicing.InvoiceTypeEnum.InvoiceWithPaymentInfo
                || model.InvoiceDetails.InvoiceType == SharedInvoicing.InvoiceTypeEnum.PaymentInfo
                || model.InvoiceDetails.InvoiceType == SharedInvoicing.InvoiceTypeEnum.RefundInvoice)
            {
                if (model.PaymentDetails?.Any() == false)
                {
                    errors.Add(new Error(nameof(model.PaymentDetails), ApiMessages.PaymentDetailsMustSpecifyAtLeastOne));
                }
            }
            else if (model.PaymentDetails?.Any() == true)
            {
                errors.Add(new Error(nameof(model.InvoiceDetails.InvoiceType), ApiMessages.PaymentDetailsNotAllowedForThisTypeOfInvoice));
            }

            if (model.PaymentDetails?.Any() == true && model.PaymentDetails.Any(p => p.PaymentType == SharedIntegration.Models.PaymentTypeEnum.Card))
            {
                CardExpirationValidator cardExpirationValidator = new CardExpirationValidator();
                foreach (var item in model.PaymentDetails.Where(p => p.PaymentType == SharedIntegration.Models.PaymentTypeEnum.Card))
                {
                    if (!cardExpirationValidator.IsValid(((CreditCardPaymentDetails)item).CardExpiration))
                    {
                        errors.Add(new Error(nameof(model.CreditCardDetails.CardExpiration), ApiMessages.CardExpirationNotValid));
                    }
                }
            }

            //If all fields are present validate the totals
            if (model.VATRate.HasValue && model.VATTotal.HasValue && model.NetTotal.HasValue)
            {
                var correctNetTotal = Math.Round(model.InvoiceAmount / (1m + model.VATRate.Value), 2, MidpointRounding.AwayFromZero);

                if (correctNetTotal != model.NetTotal.Value)
                {
                    errors.Add(new Error(nameof(model.NetTotal), Messages.ExpectedValue.Replace("@value", correctNetTotal.ToString()).Replace("@input", model.NetTotal.Value.ToString())));
                }

                var correctVatTotal = model.InvoiceAmount - correctNetTotal;

                if (correctVatTotal != model.VATTotal.Value)
                {
                    errors.Add(new Error(nameof(model.VATTotal), Messages.ExpectedValue.Replace("@value", correctVatTotal.ToString()).Replace("@input", model.VATTotal.Value.ToString())));
                }
            }
            else if (!(!model.VATRate.HasValue && !model.VATTotal.HasValue && !model.NetTotal.HasValue)) //If not all fields are null, show an error
            {
                throw new BusinessException(Messages.AllVatCalculationsMustBeSpecified);
            }

            if (errors.Count == 1)
            {
                throw new BusinessException(errors.First().Description, errors);
            }
            else if (errors.Count > 1)
            {
                throw new BusinessException(ApiMessages.ValidationErrors, errors);
            }
        }
    }
}
