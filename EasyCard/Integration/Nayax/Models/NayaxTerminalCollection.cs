using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nayax.Models
{
    public class NayaxTerminalCollection : IExternalSystemSettings
    {
        public NayaxTerminalSettings[] devices { get; set; }

        public Task<bool> Valid()
        {
            return Task.FromResult(devices?.Any() == true);
        }
    }
}
