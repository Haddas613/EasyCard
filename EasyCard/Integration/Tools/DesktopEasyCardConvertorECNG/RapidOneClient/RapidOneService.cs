using DesktopEasyCardConvertorECNG.Models;
using Microsoft.Extensions.Logging;
using RapidOne;
using RapidOne.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DesktopEasyCardConvertorECNG.RapidOneClient
{
    public class RapidOneService
    {
        private readonly RapidOneInvoicing rapidOneService;
        private readonly ILogger logger;
        private readonly AppConfig config;

        public RapidOneService(ILogger logger, AppConfig config, RapidOneInvoicing rapidOneService)
        {
            this.logger = logger;
            this.rapidOneService = rapidOneService;
            this.config = config;
        }

        public async Task<IEnumerable<ItemWithPricesDto>> GetItems()
        {
            return await rapidOneService.GetItems(config.RapidBaseUrl, config.RapidAPIKey);
        }

        public async Task CreateItem(ItemDto itemDto)
        {
            await rapidOneService.CreateItem(config.RapidBaseUrl, config.RapidAPIKey, itemDto);
        }
    }
}
