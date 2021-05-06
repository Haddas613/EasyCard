using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Shared.Enums;

namespace Transactions.Shared.Models
{
    public class BillingSchedule
    {
        public RepeatPeriodTypeEnum RepeatPeriodType { get; set; }

        public StartAtTypeEnum StartAtType { get; set; }

        public DateTime? StartAt { get; set; }

        public EndAtTypeEnum EndAtType { get; set; }

        public DateTime? EndAt { get; set; }

        public int? EndAtNumberOfPayments { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }

        public DateTime? GetNextScheduledDate(DateTime fromDate, int numberOfPaymentsCompleted = 0)
        {
            DateTime? date = RepeatPeriodType switch
            {
                RepeatPeriodTypeEnum.Montly => fromDate.AddMonths(1),
                RepeatPeriodTypeEnum.BiMontly => fromDate.AddMonths(2),
                RepeatPeriodTypeEnum.Quarter => fromDate.AddMonths(3),
                RepeatPeriodTypeEnum.Year => fromDate.AddYears(1),
                _ => null
            };

            if (EndAtType == EndAtTypeEnum.SpecifiedDate && EndAt.HasValue)
            {
                return EndAt.Value > date ? null : date;
            }

            if (EndAtType == EndAtTypeEnum.AfterNumberOfPayments && EndAtNumberOfPayments.HasValue && numberOfPaymentsCompleted > 0)
            {
                return EndAtNumberOfPayments.Value > numberOfPaymentsCompleted ? null : date;
            }

            return date;
        }

        public DateTime GetInitialScheduleDate()
        {
            DateTime date = DateTime.UtcNow;

            if (StartAtType == StartAtTypeEnum.SpecifiedDate && StartAt.HasValue)
            {
                date = StartAt.Value;
            }

            return date;
        }
    }
}
