using Merchants.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Integration
{
    public class ExternalSystem
    {
        public long ExternalSystemID { get; set; }

        public ExternalSystemTypeEnum Type { get; set; }

        /// <summary>
        /// Fully qualified Type name (used to create instances of class)
        /// </summary>
        public string InstanceTypeFullName { get; set; }

        public string Name { get; set; }

        public string Settings { get; set; }

        public byte[] UpdateTimestamp { get; set; }
    }
}
