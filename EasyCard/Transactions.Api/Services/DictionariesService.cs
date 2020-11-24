using Shared.Api.Models;
using Shared.Business.Audit;
using Shared.Helpers;
using Shared.Integration.Models;
using Shared.Integration.Resources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Dictionaries;
using Transactions.Shared.Enums;
using Transactions.Shared.Enums.Resources;
using Shared.Helpers.Resources;
using Transactions.Api.Models.Transactions.Enums;
using Shared.Integration.Models.Invoicing;
using Transactions.Api.Models.PaymentRequests.Enums;

namespace Transactions.Api.Services
{
    internal class DictionariesService
    {
        private static ConcurrentDictionary<string, TransactionsDictionaries> allResponses = new ConcurrentDictionary<string, TransactionsDictionaries>();

        public static TransactionsDictionaries GetDictionaries(string language)
        {
            var lang = language?.ToLower().Trim() ?? "en-us";
            if (!allResponses.TryGetValue(lang, out var response))
            {
                response = GetDictionariesInternal(lang);
                allResponses.TryAdd(lang, response);
            }

            return response;
        }

        private static TransactionsDictionaries GetDictionariesInternal(string language)
        {
            CultureInfo culture = new CultureInfo(language);

            var response = new TransactionsDictionaries();

            var transactionStatusEnumType = typeof(TransactionStatusEnum);
            var transactionTypeEnumType = typeof(TransactionTypeEnum);
            var spTransactionTypeEnumType = typeof(SpecialTransactionTypeEnum);
            var jDealTypeEnumType = typeof(JDealTypeEnum);
            var rejectionReasonEnumType = typeof(RejectionReasonEnum);
            var currenciesTypeEnumType = typeof(CurrencyEnum);
            var cardPresenceTypeEnumType = typeof(CardPresenceEnum);

            var filterQuickTimeEnumType = typeof(QuickTimeFilterTypeEnum);
            var filterQuickDateEnumType = typeof(QuickDateFilterTypeEnum);
            var filterQuickStatusEnumType = typeof(QuickStatusFilterTypeEnum);
            var filterDateEnumType = typeof(DateFilterTypeEnum);
            var invoiceTypeEnum = typeof(InvoiceTypeEnum);
            var invoiceStatusEnum = typeof(InvoiceStatusEnum);

            var repeatPeriodTypeEnumType = typeof(RepeatPeriodTypeEnum);
            var startAtTypeEnumType = typeof(StartAtTypeEnum);
            var endAtTypeEnumType = typeof(EndAtTypeEnum);

            var prStatusEnumType = typeof(PaymentRequestStatusEnum);
            var prQuickStatusEnumType = typeof(PayReqQuickStatusFilterTypeEnum);

            var paymentTypeEnum = typeof(PaymentTypeEnum);

            var tranStatuses = Enum.GetValues(transactionStatusEnumType).Cast<TransactionStatusEnum>()
                .ToDictionary(m => transactionStatusEnumType.GetDataContractAttrForEnum(m.ToString()), m => TransactionStatusResource.ResourceManager.GetString(m.ToString(), culture) );

            var tranTypes = Enum.GetValues(transactionTypeEnumType).Cast<TransactionTypeEnum>()
                .ToDictionary(m => transactionTypeEnumType.GetDataContractAttrForEnum(m.ToString()), m => TransactionTypeResource.ResourceManager.GetString(m.ToString(), culture) );

            var spTranTypes = Enum.GetValues(spTransactionTypeEnumType).Cast<SpecialTransactionTypeEnum>()
                .ToDictionary(m => spTransactionTypeEnumType.GetDataContractAttrForEnum(m.ToString()), m => SpecialTransactionTypeResource.ResourceManager.GetString(m.ToString(), culture) );

            var jDealTypes = Enum.GetValues(jDealTypeEnumType).Cast<JDealTypeEnum>()
                .ToDictionary(m => jDealTypeEnumType.GetDataContractAttrForEnum(m.ToString()), m => JDealTypeResource.ResourceManager.GetString(m.ToString(), culture) );

            var rejReasonTypes = Enum.GetValues(rejectionReasonEnumType).Cast<RejectionReasonEnum>()
                .ToDictionary(m => rejectionReasonEnumType.GetDataContractAttrForEnum(m.ToString()), m => RejectionReasonResource.ResourceManager.GetString(m.ToString(), culture) );

            var currTypes = Enum.GetValues(currenciesTypeEnumType).Cast<CurrencyEnum>()
                .ToDictionary(m => currenciesTypeEnumType.GetDataContractAttrForEnum(m.ToString()), m => CurrencyResource.ResourceManager.GetString(m.ToString(), culture) );

            var cardPresenceTypes = Enum.GetValues(cardPresenceTypeEnumType).Cast<CardPresenceEnum>()
                .ToDictionary(m => cardPresenceTypeEnumType.GetDataContractAttrForEnum(m.ToString()), m => CardPresenceResource.ResourceManager.GetString(m.ToString(), culture) );

            var filterQuickTimeTypes = Enum.GetValues(filterQuickTimeEnumType).Cast<QuickTimeFilterTypeEnum>()
                .ToDictionary(m => filterQuickTimeEnumType.GetDataContractAttrForEnum(m.ToString()), m => FilterEnumsResource.ResourceManager.GetString(m.ToString(), culture) );

            var filterQuickDateTypes = Enum.GetValues(filterQuickTimeEnumType).Cast<QuickDateFilterTypeEnum>()
                .ToDictionary(m => filterQuickDateEnumType.GetDataContractAttrForEnum(m.ToString()), m => FilterEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            var filterQuickStatusTypes = Enum.GetValues(filterQuickStatusEnumType).Cast<QuickStatusFilterTypeEnum>()
                .ToDictionary(m => filterQuickStatusEnumType.GetDataContractAttrForEnum(m.ToString()), m => FilterEnumsResource.ResourceManager.GetString(m.ToString(), culture) );

            var filterDateTypes = Enum.GetValues(filterDateEnumType).Cast<DateFilterTypeEnum>()
                .ToDictionary(m => filterDateEnumType.GetDataContractAttrForEnum(m.ToString()), m => FilterEnumsResource.ResourceManager.GetString(m.ToString(), culture) );

            var invoiceTypes = Enum.GetValues(invoiceTypeEnum).Cast<InvoiceTypeEnum>()
                .ToDictionary(m => invoiceTypeEnum.GetDataContractAttrForEnum(m.ToString()), m => InvoiceEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            var invoiceStatuses = Enum.GetValues(invoiceStatusEnum).Cast<InvoiceStatusEnum>()
                .ToDictionary(m => invoiceStatusEnum.GetDataContractAttrForEnum(m.ToString()), m => InvoiceEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            var repeatPeriodTypes = Enum.GetValues(repeatPeriodTypeEnumType).Cast<RepeatPeriodTypeEnum>()
                .ToDictionary(m => repeatPeriodTypeEnumType.GetDataContractAttrForEnum(m.ToString()), m => BillingDealEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            var startAtTypes = Enum.GetValues(startAtTypeEnumType).Cast<StartAtTypeEnum>()
                .ToDictionary(m => startAtTypeEnumType.GetDataContractAttrForEnum(m.ToString()), m => BillingDealEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            var endAtTypes = Enum.GetValues(endAtTypeEnumType).Cast<EndAtTypeEnum>()
                .ToDictionary(m => endAtTypeEnumType.GetDataContractAttrForEnum(m.ToString()), m => BillingDealEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            var prStatusTypes = Enum.GetValues(prStatusEnumType).Cast<PaymentRequestStatusEnum>()
                .ToDictionary(m => prStatusEnumType.GetDataContractAttrForEnum(m.ToString()), m => PaymentRequestEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            var prQuickStatusTypes = Enum.GetValues(prQuickStatusEnumType).Cast<PayReqQuickStatusFilterTypeEnum>()
                .ToDictionary(m => prQuickStatusEnumType.GetDataContractAttrForEnum(m.ToString()), m => PaymentRequestEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            var paymentTypes = Enum.GetValues(paymentTypeEnum).Cast<PaymentTypeEnum>()
                .ToDictionary(m => paymentTypeEnum.GetDataContractAttrForEnum(m.ToString()), m => PaymentTypeResource.ResourceManager.GetString(m.ToString(), culture));

            response.TransactionStatusEnum = tranStatuses;
            response.TransactionTypeEnum = tranTypes;
            response.SpecialTransactionTypeEnum = spTranTypes;
            response.JDealTypeEnum = jDealTypes;
            response.RejectionReasonEnum = rejReasonTypes;
            response.CurrencyEnum = currTypes;
            response.CardPresenceEnum = cardPresenceTypes;
            response.QuickTimeFilterTypeEnum = filterQuickTimeTypes;
            response.QuickDateFilterTypeEnum = filterQuickDateTypes;
            response.QuickStatusFilterTypeEnum = filterQuickStatusTypes;
            response.DateFilterTypeEnum = filterDateTypes;
            response.InvoiceTypeEnum = invoiceTypes;
            response.RepeatPeriodTypeEnum = repeatPeriodTypes;
            response.StartAtTypeEnum = startAtTypes;
            response.EndAtTypeEnum = endAtTypes;
            response.InvoiceStatusEnum = invoiceStatuses;
            response.PaymentRequestStatusEnum = prStatusTypes;
            response.PayReqQuickStatusFilterTypeEnum = prQuickStatusTypes;
            response.PaymentTypeEnum = paymentTypes;

            return response;
        }
    }
}
