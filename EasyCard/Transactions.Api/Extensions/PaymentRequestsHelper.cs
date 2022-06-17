using Merchants.Business.Entities.Terminal;
using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.PaymentRequests;
using Transactions.Api.Models.PaymentRequests.Enums;
using Transactions.Shared.Enums;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Extensions
{
    public static class PaymentRequestsHelper
    {
        public static PayReqQuickStatusFilterTypeEnum GetQuickStatus(this PaymentRequestStatusEnum @enum, DateTime? dueDate)
        {
            if (@enum == PaymentRequestStatusEnum.Viewed)
            {
                return PayReqQuickStatusFilterTypeEnum.Viewed;
            }

            if ((int)@enum >= 1 && (int)@enum < 4)
            {
                return PayReqQuickStatusFilterTypeEnum.Pending;
            }

            if (@enum == PaymentRequestStatusEnum.Payed)
            {
                return PayReqQuickStatusFilterTypeEnum.Completed;
            }

            if (@enum == PaymentRequestStatusEnum.Canceled || @enum == PaymentRequestStatusEnum.Rejected)
            {
                return PayReqQuickStatusFilterTypeEnum.Canceled;
            }

            if (dueDate.HasValue && dueDate < DateTime.UtcNow)
            {
                return PayReqQuickStatusFilterTypeEnum.Overdue;
            }

            if ((int)@enum < 0)
            {
                return PayReqQuickStatusFilterTypeEnum.Failed;
            }

            return PayReqQuickStatusFilterTypeEnum.Pending;
        }

        public static void UpdatePaymentRequest(this PaymentRequestCreate model, Terminal terminal)
        {
            if (!model.IssueInvoice.HasValue && terminal.CheckoutSettings.IssueInvoice == true)
            {
                model.IssueInvoice = true;
            }

            if (model.IssueInvoice.GetValueOrDefault())
            {
                if (model.InvoiceDetails == null)
                {
                    model.InvoiceDetails = new SharedIntegration.Models.Invoicing.InvoiceDetails { InvoiceType = terminal.InvoiceSettings.DefaultInvoiceType.GetValueOrDefault() };
                }

                model.InvoiceDetails.UpdateInvoiceDetails(terminal.InvoiceSettings);
            }

            if (model.AllowPinPad.GetValueOrDefault())
            {
                model.PinPadDetails = model.PinPadDetails.UpdatePinPadDetails(terminal.Integrations.FirstOrDefault(i => i.ExternalSystemID == ExternalSystemHelpers.NayaxPinpadProcessorExternalSystemID));
            }

            if (model.AllowCredit == null && (terminal.CheckoutSettings.AllowCredit == null || terminal.CheckoutSettings.AllowCredit == true))
            {
                model.AllowCredit = true;
            }

            if (model.AllowInstallments == null && (terminal.CheckoutSettings.AllowInstallments == null || terminal.CheckoutSettings.AllowInstallments == true))
            {
                model.AllowInstallments = true;
            }

            if (model.HideEmail == null && terminal.CheckoutSettings.HideEmail == true)
            {
                model.HideEmail = true;
            }

            if (model.HidePhone == null && terminal.CheckoutSettings.HidePhone == true)
            {
                model.HidePhone = true;
            }

            if (model.HideNationalID == null && terminal.CheckoutSettings.HideNationalID == true && terminal.Settings.NationalIDRequired != true)
            {
                model.HideNationalID = true;
            }

            if (!model.VATRate.HasValue)
            {
                model.VATRate = terminal.Settings.VATRate;
            }
        }
    }
}
