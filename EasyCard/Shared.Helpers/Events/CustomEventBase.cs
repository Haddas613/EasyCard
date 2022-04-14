using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.Events
{
    public class CustomEventBase
    {
        public Guid? EventID { get; set; }

        public Guid? MerchantID { get; set; }

        public Guid? TerminalID { get; set; }

        public string CorrelationId { get; set; }

        public string EventName { get; set; }

        public string EntityType { get; set; }

        public string EntityReference { get; set; }

        public string EntityExternalReference { get; set; }

        public DateTime? EventTimestamp { get; set; }

        public bool IsFailureEvent { get; set; }

        // TODO: extend by errors array
        public string ErrorMesage { get; set; }
    }
}
