using EasyInvoice.Models;
using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyInvoice
{
    public class EasyInvoiceTerminalSettings : IExternalSystemSettings
    {
        public string KeyStorePassword { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public ECInvoiceLangEnum? Lang { get; set; }
        public bool PaymentInfoAsDonation { get; set; }
        public Task<bool> Valid()
        {
            bool valid = true;

            if (string.IsNullOrEmpty(KeyStorePassword))
            {
                valid = false;
            }

            if (string.IsNullOrEmpty(UserName))
            {
                valid = false;
            }

            if (string.IsNullOrEmpty(Password))
            {
                valid = false;
            }

            return Task.FromResult(valid);
        }
    }
}
