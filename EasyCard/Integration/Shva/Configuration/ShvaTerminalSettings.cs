using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shva
{
    public class ShvaTerminalSettings : IExternalSystemSettings
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        // TODO: rename to ShvaTerminalID
        public string MerchantNumber { get; set; }

        public Task<bool> Valid()
        {
            bool valid = true;

            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(MerchantNumber))
            {
                valid = false;
            }

            return Task.FromResult(valid);
        }
    }
}
