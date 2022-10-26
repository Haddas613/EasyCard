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
using Shared.Integration.Models.Invoicing;
using Transactions.Api.Models.PaymentRequests.Enums;
using Shared.Api.Models.Enums;
using Shared.Helpers.Models;
using Shared.Helpers.Models.Resources;
using Transactions.Business.Entities;

namespace Transactions.Api.Services
{
    internal class DictionariesService
    {
        private static ConcurrentDictionary<string, TransactionsDictionaries> allResponses = new ConcurrentDictionary<string, TransactionsDictionaries>();

        public static TransactionsDictionaries GetDictionaries(CultureInfo culture)
        {
            if (culture == null)
            {
                culture = new CultureInfo("he");
            }

            if (!allResponses.TryGetValue(culture.Name, out var response))
            {
                response = GetDictionariesInternal(culture);
                allResponses.TryAdd(culture.Name, response);
            }

            return response;
        }

        private static TransactionsDictionaries GetDictionariesInternal(CultureInfo culture)
        {
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
            var solekEnum = typeof(SolekEnum);
            var cardVendorEnum = typeof(CardVendorEnum);
            var repeatPeriodTypeEnumType = typeof(RepeatPeriodTypeEnum);
            var startAtTypeEnumType = typeof(StartAtTypeEnum);
            var endAtTypeEnumType = typeof(EndAtTypeEnum);

            var prStatusEnumType = typeof(PaymentRequestStatusEnum);
            var prQuickStatusEnumType = typeof(PayReqQuickStatusFilterTypeEnum);

            var paymentTypeEnum = typeof(PaymentTypeEnum);

            var reportGranularityTypeEnum = typeof(ReportGranularityEnum);
            var quickDateFilterAltTypeEnum = typeof(QuickDateFilterAltEnum);

            var documentOriginEnumType = typeof(DocumentOriginEnum);
            var finalizationStatusEnumType = typeof(TransactionFinalizationStatusEnum);

            var propertyPresenceEnumType = typeof(PropertyPresenceEnum);

            var invoiceBillingTypeEnumType = typeof(InvoiceBillingTypeEnum);

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

            var filterQuickDateTypes = Enum.GetValues(filterQuickDateEnumType).Cast<QuickDateFilterTypeEnum>()
                .ToDictionary(m => filterQuickDateEnumType.GetDataContractAttrForEnum(m.ToString()), m => FilterEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            var filterQuickStatusTypes = Enum.GetValues(filterQuickStatusEnumType).Cast<QuickStatusFilterTypeEnum>()
                .ToDictionary(m => filterQuickStatusEnumType.GetDataContractAttrForEnum(m.ToString()), m => FilterEnumsResource.ResourceManager.GetString(m.ToString(), culture) );

            var solekEnums = Enum.GetValues(solekEnum).Cast<SolekEnum>()
              .ToDictionary(m => solekEnum.GetDataContractAttrForEnum(m.ToString()), m => CardSolekResource.ResourceManager.GetString(m.ToString(), culture));

            var cardVendorEnums = Enum.GetValues(cardVendorEnum).Cast<CardVendorEnum>()
                .ToDictionary(m => cardVendorEnum.GetDataContractAttrForEnum(m.ToString()), m => CardVendorResource.ResourceManager.GetString(m.ToString(), culture));

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

            var reportGranularityTypes = Enum.GetValues(reportGranularityTypeEnum).Cast<ReportGranularityEnum>()
                .ToDictionary(m => reportGranularityTypeEnum.GetDataContractAttrForEnum(m.ToString()), m => ReportEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            var quickDateFilterAltTypes = Enum.GetValues(quickDateFilterAltTypeEnum).Cast<QuickDateFilterAltEnum>()
                .ToDictionary(m => quickDateFilterAltTypeEnum.GetDataContractAttrForEnum(m.ToString()), m => ReportEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            var documentOriginTypes = Enum.GetValues(documentOriginEnumType).Cast<DocumentOriginEnum>()
                .ToDictionary(m => documentOriginEnumType.GetDataContractAttrForEnum(m.ToString()), m => TransactionEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            var finalizationStatusTypes = Enum.GetValues(finalizationStatusEnumType).Cast<TransactionFinalizationStatusEnum>()
                .ToDictionary(m => finalizationStatusEnumType.GetDataContractAttrForEnum(m.ToString()), m => TransactionEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            var propertyPresenceTypes = Enum.GetValues(propertyPresenceEnumType).Cast<PropertyPresenceEnum>()
               .ToDictionary(m => propertyPresenceEnumType.GetDataContractAttrForEnum(m.ToString()), m => PropertyPresenceEnumResource.ResourceManager.GetString(m.ToString(), culture));

            var invoiceBillingTypes = Enum.GetValues(invoiceBillingTypeEnumType).Cast<InvoiceBillingTypeEnum>()
               .ToDictionary(m => invoiceBillingTypeEnumType.GetDataContractAttrForEnum(m.ToString()), m => InvoiceEnumsResource.ResourceManager.GetString(m.ToString(), culture));

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
            response.SolekEnum = solekEnums;
            response.CardVendorEnum = cardVendorEnums;
            response.DateFilterTypeEnum = filterDateTypes;
            response.InvoiceTypeEnum = invoiceTypes;
            response.RepeatPeriodTypeEnum = repeatPeriodTypes;
            response.StartAtTypeEnum = startAtTypes;
            response.EndAtTypeEnum = endAtTypes;
            response.InvoiceStatusEnum = invoiceStatuses;
            response.PaymentRequestStatusEnum = prStatusTypes;
            response.PayReqQuickStatusFilterTypeEnum = prQuickStatusTypes;
            response.PaymentTypeEnum = paymentTypes;
            response.ReportGranularityEnum = reportGranularityTypes;
            response.QuickDateFilterAltEnum = quickDateFilterAltTypes;
            response.DocumentOriginEnum = documentOriginTypes;
            response.TransactionFinalizationStatusEnum = finalizationStatusTypes;
            response.PropertyPresenceEnum = propertyPresenceTypes;
            response.InvoiceBillingTypeEnum = invoiceBillingTypes;

            return response;
        }
    }
}
