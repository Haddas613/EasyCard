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
using Shared.Helpers.Resources;
using Shared.Integration.Models.Invoicing;
using MerchantProfileApi.Models.Dictionaries;
using Merchants.Shared.Enums;
using MerchantProfileApi.Models.Terminal;

namespace MerchantProfileApi.Services
{
    internal class DictionariesService
    {
        private static ConcurrentDictionary<string, MerchantDictionaries> allResponses = new ConcurrentDictionary<string, MerchantDictionaries>();

        public static MerchantDictionaries GetDictionaries(CultureInfo culture)
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

        private static MerchantDictionaries GetDictionariesInternal(CultureInfo culture)
        {
            var response = new MerchantDictionaries();

            var terminalStatusEnum = typeof(TerminalStatusEnum);
            var terminalTransmissionScheduleEnum = typeof(TerminalTransmissionScheduleEnum);

            var terminalStatuses = Enum.GetValues(terminalStatusEnum).Cast<TerminalStatusEnum>()
                .ToDictionary(m => terminalStatusEnum.GetDataContractAttrForEnum(m.ToString()), m => TerminalEnumsResource.ResourceManager.GetString(m.ToString(), culture) );

            var terminalTransmissionSchedules = Enum.GetValues(terminalTransmissionScheduleEnum).Cast<TerminalTransmissionScheduleEnum>()
                .ToDictionary(m => terminalTransmissionScheduleEnum.GetDataContractAttrForEnum(m.ToString()), m => TerminalTransmissionScheduleResource.ResourceManager.GetString(m.ToString(), culture));

            response.TerminalStatusEnum = terminalStatuses;
            response.TerminalTransmissionScheduleEnum = terminalTransmissionSchedules;

            return response;
        }
    }
}
