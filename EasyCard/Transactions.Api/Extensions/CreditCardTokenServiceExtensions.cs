using Merchants.Business.Entities.Terminal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using Transactions.Business.Services;

namespace Transactions.Api.Extensions
{
    public static class CreditCardTokenServiceExtensions
    {
        public static async Task<List<CreditCardTokenDetails>> GetTokens(this ICreditCardTokenService creditCardTokenService, Terminal terminal, Merchants.Business.Entities.Billing.Consumer consumer)
        {
            if (terminal.Settings.SharedCreditCardTokens == true)
            {
                return await creditCardTokenService.GetTokensSharedAdmin(terminal.MerchantID, terminal.TerminalID)
                    .Where(d => d.ConsumerID == consumer.ConsumerID)
                    .ToListAsync();
            }
            else
            {
                return await creditCardTokenService.GetTokens()
                    .Where(d => d.TerminalID == terminal.TerminalID && d.ConsumerID == consumer.ConsumerID)
                    .ToListAsync();
            }
        }
    }
}
