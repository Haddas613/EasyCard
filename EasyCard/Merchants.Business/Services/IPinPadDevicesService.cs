using Merchants.Business.Entities.Integration;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public interface IPinPadDevicesService : IServiceBase<PinPadDevice, string>
    {
        IQueryable<PinPadDevice> GetDevices();

        Task<PinPadDevice> GetDevice(string deviceTerminalID);

        Task AddPinPadDevice(PinPadDevice device);

        Task DeletePinPadDevice(string deviceTerminalID);
    }
}
