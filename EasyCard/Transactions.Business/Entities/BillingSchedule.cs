using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Shared.Enums;

namespace Transactions.Business.Entities
{
    public class BillingSchedule
    {
        public RepeatPeriodTypeEnum RepeatPeriodType { get; set; }

        public JObject RepeatPeriod { get; set; }

        public StartAtTypeEnum StartAtType { get; set; }

        public JObject StartAt { get; set; }

        public EndAtTypeEnum EndAtType { get; set; }

        public JObject EndAt { get; set; }
    }
}
