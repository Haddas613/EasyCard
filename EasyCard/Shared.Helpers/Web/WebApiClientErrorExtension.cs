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
