using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RapidOne
{
    public class RapidOneTerminalSettings : IExternalSystemSettings
    {
        public string BaseUrl { get; set; }
        public string Token { get; set; }
        public string Company { get; set; }
        public int Department { get; set; }
        public int Branch { get; set; }
        public string BankAccountNumber { get; set; }
        public string LedgerAccount { get; set; }
        public bool Charge { get; set; }

        public Task<bool> Valid()
        {
            bool valid = true;

            if (string.IsNullOrEmpty(BaseUrl) || string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(Company))
            {
                valid = false;
            }

            if (string.IsNullOrEmpty(BankAccountNumber) || string.IsNullOrEmpty(LedgerAccount))
            {
                valid = false;
            }

            if (Department == default || Branch == default)
            {
                valid = false;
            }

            return Task.FromResult(valid);
        }
    }
}
