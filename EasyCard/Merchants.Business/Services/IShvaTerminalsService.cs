using Merchants.Business.Entities.Integration;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public interface IShvaTerminalsService : IServiceBase<ShvaTerminal, string>
    {
        IQueryable<ShvaTerminal> GetShvaTerminals();

        Task<ShvaTerminal> GetShvaTerminal(string userName);
    }
}
