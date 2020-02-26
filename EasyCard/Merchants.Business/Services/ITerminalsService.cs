using Merchants.Business.Entities.Terminal;
using Shared.Business;
using System.Linq;

namespace Merchants.Business.Services
{
    public interface ITerminalsService : IServiceBase<Terminal>
    {
        public IQueryable<Terminal> GetTerminals();
    }
}
