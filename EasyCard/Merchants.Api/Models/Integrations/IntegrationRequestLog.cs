using Shared.Api.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Integrations
{
    public class IntegrationRequestLog
    {
        public DateTime MessageDate { get; set; }

        public string EntityID { get; set; }

        public string Request { get; set; }

        public string Response { get; set; }

        public string Address { get; set; }

        public string ResponseStatus { get; set; }

        public Guid? MerchantID { get; set; }

        public Guid? TerminalID { get; set; }

        public string CorrelationId { get; set; }

        [MetadataOptions(Hidden = true)]
        public string MessageId { get; set; }

        public string Action { get; set; }
    }
}
