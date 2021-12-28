using Shared.Business;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Integration
{
    public class PinPadDevice : IEntityBase<string>
    {
        public PinPadDevice()
        {
            Created = DateTime.UtcNow;
            PinPadDeviceID = Guid.NewGuid().GetSequentialGuid(Created);
        }

        public Guid? PinPadDeviceID { get; set; }

        public string DeviceTerminalID { get; set; }

        public string PosName { get; set; }

        /// <summary>
        /// ECNG terminal ID
        /// </summary>
        public Guid? TerminalID { get; set; }

        public DateTime Created { get; set; }

        public string CorrelationId { get; set; }

        public string GetID()
        {
            return DeviceTerminalID;
        }
    }
}
