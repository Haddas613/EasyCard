using Merchants.Business.Entities.Terminal;
using Merchants.Shared.Models;
using Shared.Business.Messages;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Shared;
using SharedHelpers = Shared.Helpers;

namespace Transactions.Api.Validation
{
    public class TransactionTerminalSettingsValidator
    {
        //TODO: validate credit card expiration
        public static void Validate(TerminalSettings terminalSettings, CreateTransactionRequest model, CreditCardTokenKeyVault token, JDealTypeEnum jDealType = JDealTypeEnum.J4, SpecialTransactionTypeEnum specialTransactionType = SpecialTransactionTypeEnum.RegularDeal, Guid? initialDealID = null)
        {
            if (terminalSettings == null)
            {
                throw new ApplicationException("Terminal settings is not present");
            }

            List<SharedHelpers.Error> errors = new List<SharedHelpers.Error>();

            if (jDealType == JDealTypeEnum.J2 && !(terminalSettings.J2Allowed == true))
            {
                errors.Add(new SharedHelpers.Error($"{nameof(terminalSettings.J2Allowed)}", Messages.J2NotAllowed));
            }
            else if (jDealType == JDealTypeEnum.J5 && !(terminalSettings.J5Allowed == true))
            {
                errors.Add(new SharedHelpers.Error($"{nameof(terminalSettings.J5Allowed)}", Messages.J5NotAllowed));
            }

            if (token != null)
            {
                if (model.CreditCardSecureDetails != null)
                {
                    throw new BusinessException(Messages.CreditCardSecureDetailsShouldBeOmitted);
                }

                //if (terminalSettings.CvvRequired)
                //{
                //    throw new BusinessException(Messages.CvvRequiredButStoredTokenCannotUseCvv);
                //}

                if (terminalSettings.NationalIDRequired == true && string.IsNullOrWhiteSpace(token.CardOwnerNationalID))
                {
                    throw new BusinessException(Messages.CardOwnerNationalIDRequiredButNotPresentInToken);
                }
            }
            else
            {
                if (!(model.PinPad ?? false))
                {
                    if (model.CreditCardSecureDetails == null)
                    {
                        throw new BusinessException(Messages.CreditCardSecureDetailsRequired);
                    }

                    if (terminalSettings.CvvRequired == true && string.IsNullOrWhiteSpace(model.CreditCardSecureDetails.Cvv))
                    {
                        errors.Add(new SharedHelpers.Error($"{nameof(model.CreditCardSecureDetails)}.{nameof(model.CreditCardSecureDetails.Cvv)}", Messages.CvvRequired));
                    }
                }

                if (terminalSettings.NationalIDRequired == true && string.IsNullOrWhiteSpace(model.CreditCardSecureDetails.CardOwnerNationalID))
                {
                    errors.Add(new SharedHelpers.Error($"{nameof(model.CreditCardSecureDetails)}.{nameof(model.CreditCardSecureDetails.CardOwnerNationalID)}", Messages.CardOwnerNationalIDRequired));
                }

                if (model.CardPresence == CardPresenceEnum.Regular && string.IsNullOrWhiteSpace(model.CreditCardSecureDetails.CardReaderInput))
                {
                    errors.Add(new SharedHelpers.Error($"{nameof(model.CreditCardSecureDetails)}.{nameof(model.CreditCardSecureDetails.CardReaderInput)}", Messages.CardReaderInputRequired));
                }
            }

            if (model.TransactionType == TransactionTypeEnum.Credit || model.TransactionType == TransactionTypeEnum.Installments)
            {
                if (model.InstallmentDetails == null)
                {
                    throw new BusinessException(Messages.InstallmentDetailsRequired);
                }

                var totalAmount = model.InstallmentDetails.InitialPaymentAmount + (model.InstallmentDetails.InstallmentPaymentAmount * (model.InstallmentDetails.NumberOfPayments - 1));

                if (totalAmount != model.InstallmentDetails.TotalAmount)
                {
                    errors.Add(new SharedHelpers.Error($"{nameof(model.InstallmentDetails)}.{nameof(model.InstallmentDetails.TotalAmount)}", Messages.TotalAmountIsInvalid));
                }
            }

            if (model.TransactionType == TransactionTypeEnum.Credit)
            {
                if (model.InstallmentDetails.NumberOfPayments < terminalSettings.MinCreditInstallments.GetValueOrDefault(3))
                {
                    errors.Add(new SharedHelpers.Error($"{nameof(model.InstallmentDetails)}.{nameof(model.InstallmentDetails.NumberOfPayments)}", string.Format(Messages.NumberOfPaymentsShouldBeGreaterThan, terminalSettings.MinCreditInstallments.GetValueOrDefault(3))));
                }

                if (terminalSettings.MaxCreditInstallments.HasValue && model.InstallmentDetails.NumberOfPayments > terminalSettings.MaxCreditInstallments.Value)
                {
                    errors.Add(new SharedHelpers.Error($"{nameof(model.InstallmentDetails)}.{nameof(model.InstallmentDetails.NumberOfPayments)}", string.Format(Messages.NumberOfPaymentsShouldBeLessThan, terminalSettings.MaxCreditInstallments)));
                }
            }
            else if (model.TransactionType == TransactionTypeEnum.Installments)
            {
                if (model.InstallmentDetails.NumberOfPayments < terminalSettings.MinInstallments.GetValueOrDefault(2))
                {
                    errors.Add(new SharedHelpers.Error($"{nameof(model.InstallmentDetails)}.{nameof(model.InstallmentDetails.NumberOfPayments)}", string.Format(Messages.NumberOfPaymentsShouldBeGreaterThan, terminalSettings.MinInstallments.GetValueOrDefault(2))));
                }

                if (terminalSettings.MaxInstallments.HasValue && model.InstallmentDetails.NumberOfPayments > terminalSettings.MaxInstallments.Value)
                {
                    errors.Add(new SharedHelpers.Error($"{nameof(model.InstallmentDetails)}.{nameof(model.InstallmentDetails.NumberOfPayments)}", string.Format(Messages.NumberOfPaymentsShouldBeLessThan, terminalSettings.MaxInstallments)));
                }
            }

            if (errors.Count == 1)
            {
                throw new BusinessException(errors.First().Description, errors);
            }
            else if (errors.Count > 1)
            {
                throw new BusinessException(ApiMessages.ValidationErrors, errors);
            }
            else
            {
                return;
            }
        }
    }
}
