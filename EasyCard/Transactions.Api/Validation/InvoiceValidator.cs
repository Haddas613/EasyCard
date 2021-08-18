using Shared.Business.Messages;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Invoicing;
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

            if (string.IsNullOrWhiteSpace(model.CardOwnerName))
            {
                errors.Add(new Error(nameof(model.CardOwnerName), ApiMessages.Required));
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
                errors.Add(new Error(nameof(model.CardOwnerName), ApiMessages.PaymentDetailsNotAllowedForThisTypeOfInvoice));
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
