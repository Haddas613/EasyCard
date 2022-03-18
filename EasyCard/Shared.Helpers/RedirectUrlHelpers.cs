using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Helpers
{
    public static class RedirectUrlHelpers
    {
        public static Uri CheckRedirectUrl(this string redirectUrl)
        {
            Uri url;

            if (!Uri.TryCreate(redirectUrl, UriKind.Absolute, out url))
            {
                throw new BusinessException("Given string is not valid url");
            }

            if (!url.IsAbsoluteUri)
            {
                throw new BusinessException("Redirect url should be absolute");
            }

            if (url.Scheme != Uri.UriSchemeHttps)
            {
                throw new BusinessException("Only https supported");
            }

            return url;
        }

        public static bool CheckRedirectUrl(this string redirectUrlBase, string redirectUrlPartial)
        {
            var urlBase = redirectUrlBase.CheckRedirectUrl();
            var urlPartial = redirectUrlPartial.CheckRedirectUrl();

            return !urlBase.IsBaseOf(urlPartial);
        }

        public static void CheckRedirectUrls(this IEnumerable<string> baseRedirectUrls, string redirectUrlPartial)
        {
            //Workaround so Ecwid works without additional setup
            if ("https://app.ecwid.com".CheckRedirectUrl(redirectUrlPartial))
            {
                return;
            }

            if (baseRedirectUrls == null)
            {
                throw new BusinessException("Please configure redirect urls");
            }

            var baseUrls = baseRedirectUrls.Select(d => d.CheckRedirectUrl());
            var urlPartial = redirectUrlPartial.CheckRedirectUrl();

            if (!baseUrls.Any(d => d.IsBaseOf(urlPartial)))
            {
                throw new BusinessException("Given string is not valid redirect url");
            }
        }
    }
}
