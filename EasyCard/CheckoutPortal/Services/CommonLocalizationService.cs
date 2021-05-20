using CheckoutPortal.Resources;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CheckoutPortal.Services
{
    public class CommonLocalizationService
    {
        private readonly System.Resources.ResourceManager localizer;

        public CommonLocalizationService()
        {
            localizer = CommonResources.ResourceManager;
        }

        public string Get(string key)
        {
            return localizer.GetString(key);
        }
    }
}
