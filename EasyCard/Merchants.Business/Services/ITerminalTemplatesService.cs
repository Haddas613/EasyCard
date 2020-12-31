using Merchants.Business.Entities.Terminal;
using Merchants.Business.Entities.User;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public interface ITerminalTemplatesService : IServiceBase<TerminalTemplate, long>
    {
        public IQueryable<TerminalTemplate> GetTerminals();
    }
}
