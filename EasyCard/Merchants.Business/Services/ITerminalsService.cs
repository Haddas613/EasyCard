using Merchants.Business.Entities.Terminal;
using Shared.Business;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public interface ITerminalsService : IServiceBase<Terminal>
    {
        public IQueryable<Terminal> GetTerminals();

        public Task LinkUserToTerminal(string userID, long terminalID);

        public Task UnLinkUserFromTerminal(string userID, long terminalID);
    }
}
