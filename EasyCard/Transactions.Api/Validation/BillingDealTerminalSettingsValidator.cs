using Merchants.Shared.Models;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Billing;
using Transactions.Shared;
using Shared.Business.Messages;
using Transactions.Shared.Models;
using Transactions.Shared.Enums;
using Shared.Api.Models.Enums;
using Shared.Api.Models;
using SharedHelpers = Shared.Helpers;

namespace Transactions.Api.Validation
{
    public class BillingDealTerminalSettingsValidator
    {
        public static DateTime? ValidateSchedue(BillingSchedule billingSchedule, DateTime? existingTransaction)
        {
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;

            if (existingTransaction.HasValue)
            {
                var lastTransactionDate = TimeZoneInfo.ConvertTimeFromUtc(existingTransaction.Value, UserCultureInfo.TimeZone).Date;

                if (billingSchedule?.StartAtType == StartAtTypeEnum.SpecifiedDate && billingSchedule?.StartAt.HasValue == true)
                {
                    if (lastTransactionDate.Month == billingSchedule.StartAt.Value.Month && lastTransactionDate.Year == billingSchedule.StartAt.Value.Year)
                    {
                        throw new BusinessException($"{nameof(billingSchedule.StartAt)} must be bigger than (or equal) {new DateTime(lastTransactionDate.Year, lastTransactionDate.Month, 1).AddMonths(1)}");
                    }
                    else if (billingSchedule.StartAt.Value < today)
                    {
                        throw new BusinessException($"{nameof(billingSchedule.StartAt)} must be bigger than (or equal) {today}");
                    }
                    else
                    {
                        return billingSchedule.GetInitialScheduleDate();
                    }
                }
                else
                {
                    if (lastTransactionDate.Month == today.Month && lastTransactionDate.Year == today.Year)
                    {
                        throw new BusinessException($"{nameof(billingSchedule.StartAt)} must be bigger than (or equal) {new DateTime(lastTransactionDate.Year, lastTransactionDate.Month, 1).AddMonths(1)}");
                    }
                    else
                    {
                        return billingSchedule.GetInitialScheduleDate();
                    }
                }
            }
            else
            {
                // no transactions yet generated from billing

                if (billingSchedule?.StartAtType == StartAtTypeEnum.SpecifiedDate && billingSchedule?.StartAt.HasValue == true)
                {
                    if (billingSchedule?.StartAt >= today)
                    {
                        return billingSchedule.GetInitialScheduleDate();
                    }
                    else
                    {
                        throw new BusinessException($"{nameof(billingSchedule.StartAt)} must be bigger than (or equal) {today}");
                    }
                }
                else
                {
                    return billingSchedule.GetInitialScheduleDate();
                }
            }
        }

        public static void Validate(TerminalSettings terminalSettings, BillingDealRequest request)
        {
            if (terminalSettings == null)
            {
                throw new ApplicationException("Terminal settings is not present");
            }

            List<SharedHelpers.Error> errors = new List<SharedHelpers.Error>();

            //If all fields are present validate the totals
            if (request.VATRate.HasValue && request.VATTotal.HasValue && request.NetTotal.HasValue)
            {
                var correctNetTotal = Math.Round(request.TransactionAmount / (1m + request.VATRate.Value), 2, MidpointRounding.AwayFromZero);

                if (correctNetTotal != request.NetTotal.Value)
                {
                    errors.Add(new Error(nameof(request.NetTotal), Messages.ExpectedValue.Replace("@value", correctNetTotal.ToString()).Replace("@input", request.NetTotal.Value.ToString())));
                }

                var correctVatTotal = request.TransactionAmount - correctNetTotal;

                if (correctVatTotal != request.VATTotal.Value)
                {
                    errors.Add(new Error(nameof(request.VATTotal), Messages.ExpectedValue.Replace("@value", correctVatTotal.ToString()).Replace("@input", request.VATTotal.Value.ToString())));
                }
            }

            //If not all fields are null, show an error
            else if (!(!request.VATRate.HasValue && !request.VATTotal.HasValue && !request.NetTotal.HasValue))
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

        public static void Validate(TerminalSettings terminalSettings, BillingDealUpdateRequest request)
        {
            if (terminalSettings == null)
            {
                throw new ApplicationException("Terminal settings is not present");
            }

            List<SharedHelpers.Error> errors = new List<SharedHelpers.Error>();

            //If all fields are present validate the totals
            if (request.VATRate.HasValue && request.VATTotal.HasValue && request.NetTotal.HasValue)
            {
                var correctNetTotal = Math.Round(request.TransactionAmount / (1m + request.VATRate.Value), 2, MidpointRounding.AwayFromZero);

                if (correctNetTotal != request.NetTotal.Value)
                {
                    errors.Add(new Error(nameof(request.NetTotal), Messages.ExpectedValue.Replace("@value", correctNetTotal.ToString()).Replace("@input", request.NetTotal.Value.ToString())));
                }

                var correctVatTotal = request.TransactionAmount - correctNetTotal;

                if (correctVatTotal != request.VATTotal.Value)
                {
                    errors.Add(new Error(nameof(request.VATTotal), Messages.ExpectedValue.Replace("@value", correctVatTotal.ToString()).Replace("@input", request.VATTotal.Value.ToString())));
                }
            }

            //If not all fields are null, show an error
            else if (!(!request.VATRate.HasValue && !request.VATTotal.HasValue && !request.NetTotal.HasValue))
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

        public static void Validate(TerminalSettings terminalSettings, BillingDealInvoiceOnlyRequest request)
        {
            if (terminalSettings == null)
            {
                throw new ApplicationException("Terminal settings is not present");
            }

            List<SharedHelpers.Error> errors = new List<SharedHelpers.Error>();

            //If all fields are present validate the totals
            if (request.VATRate.HasValue && request.VATTotal.HasValue && request.NetTotal.HasValue)
            {
                var correctNetTotal = Math.Round(request.TransactionAmount / (1m + request.VATRate.Value), 2, MidpointRounding.AwayFromZero);

                if (correctNetTotal != request.NetTotal.Value)
                {
                    errors.Add(new Error(nameof(request.NetTotal), Messages.ExpectedValue.Replace("@value", correctNetTotal.ToString()).Replace("@input", request.NetTotal.Value.ToString())));
                }

                var correctVatTotal = request.TransactionAmount - correctNetTotal;

                if (correctVatTotal != request.VATTotal.Value)
                {
                    errors.Add(new Error(nameof(request.VATTotal), Messages.ExpectedValue.Replace("@value", correctVatTotal.ToString()).Replace("@input", request.VATTotal.Value.ToString())));
                }
            }

            //If not all fields are null, show an error
            else if (!(!request.VATRate.HasValue && !request.VATTotal.HasValue && !request.NetTotal.HasValue))
            {
                throw new BusinessException(Messages.AllVatCalculationsMustBeSpecified);
            }

            if (request.PaymentDetails?.Any() == false)
            {
                errors.Add(new Error(nameof(request.PaymentDetails), ApiMessages.PaymentDetailsMustSpecifyAtLeastOne));
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

        public static void Validate(TerminalSettings terminalSettings, BillingDealInvoiceOnlyUpdateRequest request)
        {
            if (terminalSettings == null)
            {
                throw new ApplicationException("Terminal settings is not present");
            }

            List<SharedHelpers.Error> errors = new List<SharedHelpers.Error>();

            //If all fields are present validate the totals
            if (request.VATRate.HasValue && request.VATTotal.HasValue && request.NetTotal.HasValue)
            {
                var correctNetTotal = Math.Round(request.TransactionAmount / (1m + request.VATRate.Value), 2, MidpointRounding.AwayFromZero);

                if (correctNetTotal != request.NetTotal.Value)
                {
                    errors.Add(new Error(nameof(request.NetTotal), Messages.ExpectedValue.Replace("@value", correctNetTotal.ToString()).Replace("@input", request.NetTotal.Value.ToString())));
                }

                var correctVatTotal = request.TransactionAmount - correctNetTotal;

                if (correctVatTotal != request.VATTotal.Value)
                {
                    errors.Add(new Error(nameof(request.VATTotal), Messages.ExpectedValue.Replace("@value", correctVatTotal.ToString()).Replace("@input", request.VATTotal.Value.ToString())));
                }
            }

            //If not all fields are null, show an error
            else if (!(!request.VATRate.HasValue && !request.VATTotal.HasValue && !request.NetTotal.HasValue))
            {
                throw new BusinessException(Messages.AllVatCalculationsMustBeSpecified);
            }

            if (request.PaymentDetails?.Any() == false)
            {
                errors.Add(new Error(nameof(request.PaymentDetails), ApiMessages.PaymentDetailsMustSpecifyAtLeastOne));
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
