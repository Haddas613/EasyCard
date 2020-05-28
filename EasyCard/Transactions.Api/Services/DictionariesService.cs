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
            var filterQuickStatusEnumType = typeof(QuickStatusFilterTypeEnum);
            var filterDateEnumType = typeof(DateFilterTypeEnum);

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
                .ToDictionary(m => filterQuickTimeEnumType.GetDataContractAttrForEnum(m.ToString()), m => CardPresenceResource.ResourceManager.GetString(m.ToString(), culture) );

            var filterQuickStatusTypes = Enum.GetValues(filterQuickStatusEnumType).Cast<QuickStatusFilterTypeEnum>()
                .ToDictionary(m => filterQuickStatusEnumType.GetDataContractAttrForEnum(m.ToString()), m => CardPresenceResource.ResourceManager.GetString(m.ToString(), culture) );

            var filterDateTypes = Enum.GetValues(filterDateEnumType).Cast<DateFilterTypeEnum>()
                .ToDictionary(m => filterDateEnumType.GetDataContractAttrForEnum(m.ToString()), m => CardPresenceResource.ResourceManager.GetString(m.ToString(), culture) );

            response.TransactionStatusEnum = tranStatuses;
            response.TransactionTypeEnum = tranTypes;
            response.SpecialTransactionTypeEnum = spTranTypes;
            response.JDealTypeEnum = jDealTypes;
            response.RejectionReasonEnum = rejReasonTypes;
            response.CurrencyEnum = currTypes;
            response.CardPresenceEnum = cardPresenceTypes;
            response.QuickTimeFilterTypeEnum = filterQuickTimeTypes;
            response.QuickStatusFilterTypeEnum = filterQuickStatusTypes;
            response.DateFilterTypeEnum = filterDateTypes;

            return response;
        }
    }
}
