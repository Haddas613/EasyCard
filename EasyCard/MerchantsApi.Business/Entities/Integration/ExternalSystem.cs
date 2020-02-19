using System;
using System.Collections.Generic;
using System.Text;

namespace MerchantsApi.Business.Entities
{
    public class ExternalSystem
    {
        public long ExternalSystemID { get; set; }
        public ExternalSystemTypeEnum Type { get; set; }
        public string Name { get; set; }
        public string Settings { get; set; }
        public byte[] UpdateTimestamp { get; set; }
    }
}
