using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClearingHouse
{
    public class ClearingHouseTerminalSettings : IExternalSystemSettings
    {
        public string MerchantReference { get; set; }

        // TODO: this is temporary implementation
        public string ShvaTerminalReference { get; set; }

        public long? MerchantID { get; set; }

        public Task<bool> Valid()
        {
            bool valid = true;

            if (string.IsNullOrEmpty(MerchantReference))
            {
                valid = false;
            }

            if (string.IsNullOrEmpty(ShvaTerminalReference))
            {
                valid = false;
            }

            if (MerchantID.GetValueOrDefault() == default)
            {
                valid = false;
            }

            return Task.FromResult(valid);
        }
    }
}
