using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nayax
{
    public class NayaxTerminalSettings : IExternalSystemSettings
    {
        public string TerminalID { get; set; }

        public string PosName { get; set; }

        public Task<bool> Valid()
        {
            bool valid = true;

            if (string.IsNullOrEmpty(TerminalID))
            {
                valid = false;
            }

            if (string.IsNullOrEmpty(PosName))
            {
                valid = false;
            }

            return Task.FromResult(valid);
        }
    }
}
