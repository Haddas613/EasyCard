using Shared.Business.Messages;
using Shared.Helpers;
using System.Collections.Generic;
using System.Linq;
using Transactions.Api.Models.PaymentRequests;
using Transactions.Api.Resources;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Validation
{
    public class PaymentRequestValidator
    {
        public static void ValidatePaymentRequest(PaymentRequestCreate model)
        {
            List<Error> errors = new List<Error>();

            if (model.TransactionType == null)
            {
                if (model.InstallmentDetails?.NumberOfPayments > 1)
                {
                    model.TransactionType = SharedIntegration.Models.TransactionTypeEnum.Installments;
                }
                else
                {
                    model.TransactionType = SharedIntegration.Models.TransactionTypeEnum.RegularDeal;
                }
            }

            if (model.AllowInstallments == false && model.TransactionType == SharedIntegration.Models.TransactionTypeEnum.Installments)
            {
                errors.Add(new Error(nameof(model.AllowInstallments), ValidationMessages.AllowInstallmentsError));
            }

            if (model.AllowCredit == false && model.TransactionType == SharedIntegration.Models.TransactionTypeEnum.Credit)
            {
                errors.Add(new Error(nameof(model.AllowCredit), ValidationMessages.AllowCreditError));
            }

            if ((model.TransactionType == SharedIntegration.Models.TransactionTypeEnum.RegularDeal || model.TransactionType == SharedIntegration.Models.TransactionTypeEnum.Immediate) && model.InstallmentDetails?.NumberOfPayments > 1)
            {
                errors.Add(new Error(nameof(model.InstallmentDetails.NumberOfPayments), ValidationMessages.NumberOfPaymentsError));
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
