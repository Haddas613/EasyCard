using Merchants.Business.Data;
using Merchants.Business.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public class PinPadDevicesService : ServiceBase<PinPadDevice, string>, IPinPadDevicesService
    {
        private readonly MerchantsContext context;

        public PinPadDevicesService(MerchantsContext context)
            : base(context)
        {
            this.context = context;
        }

        public async Task AddPinPadDevice(PinPadDevice device)
        {
            var exists = await context.PinPadDevices.AnyAsync(d => d.DeviceTerminalID == device.DeviceTerminalID);

            if (exists)
            {
                throw new Exception("Device is already paired. Please delete it first");
            }

            var newDevice = new PinPadDevice
            {
                DeviceTerminalID = device.DeviceTerminalID,
                PosName = device.PosName,
                TerminalID = device.TerminalID,
                CorrelationId = device.CorrelationId
            };

            context.PinPadDevices.Add(newDevice);
            await context.SaveChangesAsync();
        }

        public async Task DeletePinPadDevice(string deviceTerminalID)
        {
            var dbEntity = await context.PinPadDevices.FirstOrDefaultAsync(d => d.DeviceTerminalID == deviceTerminalID);

            if (dbEntity != null)
            {
                context.PinPadDevices.Remove(dbEntity);
                await context.SaveChangesAsync();
            }
        }

        public Task<PinPadDevice> GetDevice(string deviceTerminalID)
            => context.PinPadDevices.FirstOrDefaultAsync(t => t.DeviceTerminalID == deviceTerminalID);

        public IQueryable<PinPadDevice> GetDevices()
            => context.PinPadDevices.AsNoTracking();
    }
}
