using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.Security
{
    public interface ITerminalApiKeyTokenServiceFactory
    {
        IWebApiClientTokenService CreateTokenService(string privateKey);
    }
}
