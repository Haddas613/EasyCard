using Shared.Api.Models;
using Shared.Business.Audit;
using Shared.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Shared.Helpers.Resources;
using Merchants.Api.Models.Dictionaries;
using Merchants.Shared.Enums;
using Merchants.Api.Models.Terminal;
using Merchants.Api.Models.User;

namespace Merchants.Api.Services
{
    internal class DictionariesService
    {
        private static ConcurrentDictionary<string, MerchantsDictionaries> allResponses = new ConcurrentDictionary<string, MerchantsDictionaries>();

        public static MerchantsDictionaries GetDictionaries(string language)
        {
            var lang = language?.ToLower().Trim() ?? "en-us";
            if (!allResponses.TryGetValue(lang, out var response))
            {
                response = GetDictionariesInternal(lang);
                allResponses.TryAdd(lang, response);
            }

            return response;
        }

        private static MerchantsDictionaries GetDictionariesInternal(string language)
        {
            CultureInfo culture = new CultureInfo(language);

            var response = new MerchantsDictionaries();

            var transactionStatusEnumType = typeof(TerminalStatusEnum);

            var userStatusEnumType = typeof(UserStatusEnum);

            var termStatuses = Enum.GetValues(transactionStatusEnumType).Cast<TerminalStatusEnum>()
                .ToDictionary(m => transactionStatusEnumType.GetDataContractAttrForEnum(m.ToString()), m => TerminalStatusEnumResource.ResourceManager.GetString(m.ToString(), culture) );

            var userStatuses = Enum.GetValues(userStatusEnumType).Cast<UserStatusEnum>()
                .ToDictionary(m => userStatusEnumType.GetDataContractAttrForEnum(m.ToString()), m => UserEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            response.TerminalStatusEnum = termStatuses;
            response.UserStatusEnum = userStatuses;

            return response;
        }
    }
}
