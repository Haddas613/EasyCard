using Merchants.Business.Entities.Terminal;
using Merchants.Business.Extensions;
using Shared.Integration;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions;
using Transactions.Business.Entities;

namespace Transactions.Api.Extensions
{
    public static class TransactionStatusExtensions
    {
        public static void UpdateAggregatorDetails(this PaymentTransaction tr, TransactionResponse transaction)
        {
            // TODO: find another way to map it
            if (tr.AggregatorID == 60)
            {
                transaction.ClearingHouseTransactionDetails = null;
            }
            else if (tr.AggregatorID == 10)
            {
                transaction.UpayTransactionDetails = null;
            }
            else
            {
                transaction.ClearingHouseTransactionDetails = null;
                transaction.UpayTransactionDetails = null;
            }
        }

        public static bool AllowRefund(this PaymentTransaction transaction, Terminal terminal)
        {
            if (transaction.AllowRefund)
            {
                return terminal.FeatureEnabled(Merchants.Shared.Enums.FeatureEnum.Chargebacks);
            }
            else
            {
                return false;
            }
        }

        public static bool AllowInvoiceCreation(this PaymentTransaction transaction, Terminal terminal)
        {
            if (transaction.AllowInvoiceCreation)
            {
                return terminal.IntegrationEnabled(ExternalSystemHelpers.ECInvoiceExternalSystemID)
                    || terminal.IntegrationEnabled(ExternalSystemHelpers.RapidOneInvoicingExternalSystemID);
            }
            else
            {
                return false;
            }
        }

        public static bool AllowTransmissionCancellation(this PaymentTransaction transaction, IAggregator aggregator)
        {
            if (transaction.AllowTransmissionCancellation)
            {
                return aggregator == null ? true : aggregator.AllowTransmissionCancellation();
            }
            else
            {
                return false;
            }
        }

        public static bool AllowTransmission(this PaymentTransaction transaction)
        {
            if (transaction.AllowTransmission)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
