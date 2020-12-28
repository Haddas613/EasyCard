using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers
{
    public static class WebApiClientErrorExtension
    {
        public static T TryConvert<T>(this WebApiClientErrorException webApiClientErrorEx, T defaultValue)
        {
            if (string.IsNullOrEmpty(webApiClientErrorEx.Response))
            {
                return defaultValue;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(webApiClientErrorEx.Response);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}
