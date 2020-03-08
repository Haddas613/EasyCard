using System.Threading.Tasks;
using IdentityModel.Client;

namespace Shared.Helpers.Security
{
    public interface IWebApiClientTokenService
    {
        Task<TokenResponse> GetToken();
    }
}
