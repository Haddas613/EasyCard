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
using Microsoft.Extensions.Logging;
using Merchants.Api.Models.System;

namespace Merchants.Api.Services
{
    internal class DictionariesService
    {
        private static ConcurrentDictionary<string, MerchantsDictionaries> allResponses = new ConcurrentDictionary<string, MerchantsDictionaries>();

        public static MerchantsDictionaries GetDictionaries(CultureInfo culture)
        {
            if (culture == null)
            {
                culture = new CultureInfo("en-IL");
            }

            if (!allResponses.TryGetValue(culture.Name, out var response))
            {
                response = GetDictionariesInternal(culture);
                allResponses.TryAdd(culture.Name, response);
            }

            return response;
        }

        private static MerchantsDictionaries GetDictionariesInternal(CultureInfo culture)
        {

            var response = new MerchantsDictionaries();

            var transactionStatusEnumType = typeof(TerminalStatusEnum);

            var userStatusEnumType = typeof(UserStatusEnum);

            var logLevelsType = typeof(LogLevel);

            var operationCodesType = typeof(OperationCodesEnum);

            var termStatuses = Enum.GetValues(transactionStatusEnumType).Cast<TerminalStatusEnum>()
                .ToDictionary(m => transactionStatusEnumType.GetDataContractAttrForEnum(m.ToString()), m => TerminalStatusEnumResource.ResourceManager.GetString(m.ToString(), culture) );

            var userStatuses = Enum.GetValues(userStatusEnumType).Cast<UserStatusEnum>()
                .ToDictionary(m => userStatusEnumType.GetDataContractAttrForEnum(m.ToString()), m => UserEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            var logLevels = Enum.GetValues(logLevelsType).Cast<LogLevel>()
                .ToDictionary(m => logLevelsType.GetDataContractAttrForEnum(m.ToString()), m => SystemEnumsResource.ResourceManager.GetString(m.ToString(), culture));

            var operationCodes = Enum.GetValues(operationCodesType).Cast<OperationCodesEnum>()
                .ToDictionary(m => operationCodesType.GetDataContractAttrForEnum(m.ToString()), m => m.ToString());

            response.TerminalStatusEnum = termStatuses;
            response.UserStatusEnum = userStatuses;
            response.LogLevelsEnum = logLevels;
            response.OperationCodesEnum = operationCodes;

            return response;
        }
    }
}
