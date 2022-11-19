using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Api.Models.Binding;
using Shared.Helpers;
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

        [JsonConverter(typeof(TrimmingDateTimeConverter))]
        public DateTime? StartAt { get; set; }

        public EndAtTypeEnum EndAtType { get; set; }

        [JsonConverter(typeof(TrimmingDateTimeConverter))]
        public DateTime? EndAt { get; set; }

        public int? EndAtNumberOfPayments { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }

        public DateTime? GetNextScheduledDate(DateTime fromDate, int numberOfPaymentsCompleted)
        {
            DateTime? date = RepeatPeriodType switch
            {
                RepeatPeriodTypeEnum.Monthly => fromDate.AddMonths(1),
                RepeatPeriodTypeEnum.BiMonthly => fromDate.AddMonths(2),
                RepeatPeriodTypeEnum.Quarter => fromDate.AddMonths(3),
                RepeatPeriodTypeEnum.HalfYear => fromDate.AddMonths(6),
                RepeatPeriodTypeEnum.Year => fromDate.AddYears(1),
                _ => null
            };

            if (date == null)
            {
                return null;
            }

            if (StartAt.HasValue)
            {
                var daysInMonth = DateTime.DaysInMonth(date.Value.Year, date.Value.Month);
                var day = StartAt.Value.Day;
                if (day > daysInMonth)
                {
                    day = daysInMonth;
                }
                date = new DateTime(date.Value.Year, date.Value.Month, day);
            }

            if (EndAtType == EndAtTypeEnum.SpecifiedDate && EndAt.HasValue)
            {
                return date > EndAt.Value ? null : date;
            }

            if (EndAtType == EndAtTypeEnum.AfterNumberOfPayments && EndAtNumberOfPayments.HasValue && numberOfPaymentsCompleted > 0)
            {
                return numberOfPaymentsCompleted > EndAtNumberOfPayments.Value ? null : date;
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

            return TimeZoneInfo.ConvertTimeFromUtc(date, UserCultureInfo.TimeZone).Date;
        }

        public void Validate(DateTime? existingTransactionTimestamp, bool startDateChanged)
        {
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;

            //if (EndAtType == EndAtTypeEnum.SpecifiedDate && EndAt.HasValue == true && EndAt < today)
            //{
            //    throw new BusinessException($"{nameof(EndAt)} must be bigger than (or equal) {today}");
            //}

            if (EndAtType == EndAtTypeEnum.SpecifiedDate && EndAt.HasValue == true && StartAtType == StartAtTypeEnum.SpecifiedDate && StartAt.HasValue == true && EndAt < StartAt)
            {
                throw new BusinessException($"{nameof(EndAt)} must be bigger than (or equal) {StartAt}");
            }

            if (EndAtType == EndAtTypeEnum.AfterNumberOfPayments && EndAtNumberOfPayments.GetValueOrDefault() <= 0)
            {
                EndAt = null;
                EndAtType = EndAtTypeEnum.Never;
                EndAtNumberOfPayments = null;
            }

            if (EndAtType == EndAtTypeEnum.SpecifiedDate && EndAt == null)
            {
                EndAt = null;
                EndAtType = EndAtTypeEnum.Never;
                EndAtNumberOfPayments = null;
            }

            if (existingTransactionTimestamp.HasValue && startDateChanged)
            {
                var lastTransactionDate = TimeZoneInfo.ConvertTimeFromUtc(existingTransactionTimestamp.Value, UserCultureInfo.TimeZone).Date;

                if (StartAtType == StartAtTypeEnum.SpecifiedDate && StartAt.HasValue == true)
                {
                    if (lastTransactionDate.Month == StartAt.Value.Month && lastTransactionDate.Year == StartAt.Value.Year)
                    {
                        throw new BusinessException($"{nameof(StartAt)} must be bigger than (or equal) {new DateTime(lastTransactionDate.Year, lastTransactionDate.Month, 1).AddMonths(1)}");
                    }
                    else if (StartAt.Value < today)
                    {
                        throw new BusinessException($"{nameof(StartAt)} must be bigger than (or equal) {today}");
                    }
                }
                else
                {
                    if (lastTransactionDate.Month == today.Month && lastTransactionDate.Year == today.Year)
                    {
                        throw new BusinessException($"{nameof(StartAt)} must be bigger than (or equal) {new DateTime(lastTransactionDate.Year, lastTransactionDate.Month, 1).AddMonths(1)}");
                    }
                }
            }
        }

        public override bool Equals(object obj)
        {
            var c = obj as BillingSchedule;
            if (c == null)
                return false;

            return RepeatPeriodType == c.RepeatPeriodType 
                && StartAtType == c.StartAtType
                && EndAtType == c.EndAtType 
                && StartAt == c.StartAt
                && EndAt == c.EndAt 
                && EndAtNumberOfPayments == c.EndAtNumberOfPayments;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
