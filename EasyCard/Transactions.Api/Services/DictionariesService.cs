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

            var tranStatuses = Enum.GetValues(transactionStatusEnumType).Cast<TransactionStatusEnum>()
                .Select(m => new DictionarySummary<string> { Code = transactionStatusEnumType.GetDataContractAttrForEnum(m.ToString()), Description = TransactionStatusResource.ResourceManager.GetString(m.ToString(), culture) });

            var tranTypes = Enum.GetValues(transactionTypeEnumType).Cast<TransactionTypeEnum>()
                .Select(m => new DictionarySummary<string> { Code = transactionTypeEnumType.GetDataContractAttrForEnum(m.ToString()), Description = TransactionTypeResource.ResourceManager.GetString(m.ToString(), culture) });

            var spTranTypes = Enum.GetValues(spTransactionTypeEnumType).Cast<SpecialTransactionTypeEnum>()
                .Select(m => new DictionarySummary<string> { Code = spTransactionTypeEnumType.GetDataContractAttrForEnum(m.ToString()), Description = SpecialTransactionTypeResource.ResourceManager.GetString(m.ToString(), culture) });

            var jDealTypes = Enum.GetValues(jDealTypeEnumType).Cast<JDealTypeEnum>()
                .Select(m => new DictionarySummary<short> { Code = (short)m, Description = JDealTypeResource.ResourceManager.GetString(m.ToString(), culture) });

            var rejReasonTypes = Enum.GetValues(rejectionReasonEnumType).Cast<RejectionReasonEnum>()
                .Select(m => new DictionarySummary<short> { Code = (short)m, Description = RejectionReasonResource.ResourceManager.GetString(m.ToString(), culture) });

            var currTypes = Enum.GetValues(currenciesTypeEnumType).Cast<CurrencyEnum>()
                .Select(m => new DictionarySummary<string> { Code = currenciesTypeEnumType.GetDataContractAttrForEnum(m.ToString()), Description = CurrencyResource.ResourceManager.GetString(m.ToString(), culture) });

            var cardPresenceTypes = Enum.GetValues(cardPresenceTypeEnumType).Cast<CardPresenceEnum>()
                .Select(m => new DictionarySummary<string> { Code = cardPresenceTypeEnumType.GetDataContractAttrForEnum(m.ToString()), Description = CardPresenceResource.ResourceManager.GetString(m.ToString(), culture) });

            response.TransactionStatuses = tranStatuses;
            response.TransactionTypes = tranTypes;
            response.SpecialTransactionTypes = spTranTypes;
            response.JDealTypes = jDealTypes;
            response.RejectionReasons = rejReasonTypes;
            response.Currencies = currTypes;
            response.CardPresences = cardPresenceTypes;

            return response;
        }
    }
}
