using IdentityServer.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Services
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
