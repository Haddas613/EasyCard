using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.User;
using Merchants.Business.Models.Merchant;
using Merchants.Shared.Enums;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public interface IMerchantsService : IServiceBase<Merchant, Guid>
    {
        IQueryable<Merchant> GetMerchants();

        IQueryable<MerchantHistory> GetMerchantHistories();

        IQueryable<UserTerminalMapping> GetMerchantUsers();

        IQueryable<UserInfo> GetMerchantUsers(Guid merchantID);

        Task LinkUserToMerchant(UserInfo userID, Guid merchantID, IDbContextTransaction dbTransaction = null);

        Task UnLinkUserFromMerchant(Guid userID, Guid merchantID, IDbContextTransaction dbTransaction = null);

        Task UpdateUserStatus(UpdateUserStatusData data, IDbContextTransaction dbTransaction = null);

        Task UpdateUserRoles(Guid userID, ICollection<string> roles);
    }
}
