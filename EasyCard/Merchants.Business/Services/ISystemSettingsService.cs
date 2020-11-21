using Merchants.Business.Entities.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public interface ISystemSettingsService
    {
        Task<SystemSettings> GetSystemSettings();

        Task UpdateSystemSettings(SystemSettings entity);
    }
}
