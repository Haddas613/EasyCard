using Merchants.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Shared.Extensions
{
    public static class TerminalTransmissionScheduleExtensions
    {
        public static bool ScheduleApply(this TerminalTransmissionScheduleEnum @enum, DateTime @for) =>
            @enum switch
            {
                TerminalTransmissionScheduleEnum.NotSpecified => true,
                TerminalTransmissionScheduleEnum.From12amTo3am when @for.Hour < 3 => true,
                TerminalTransmissionScheduleEnum.From3amTo6am when @for.Hour >= 3 && @for.Hour < 6 => true,
                TerminalTransmissionScheduleEnum.From6amTo9am when @for.Hour >= 6 && @for.Hour < 9 => true,
                TerminalTransmissionScheduleEnum.From9amTo12pm when @for.Hour >= 9 && @for.Hour < 12 => true,
                TerminalTransmissionScheduleEnum.From12pmTo3pm when @for.Hour >= 12 && @for.Hour < 15 => true,
                TerminalTransmissionScheduleEnum.From3pmTo6pm when @for.Hour >= 15 && @for.Hour < 18 => true,
                TerminalTransmissionScheduleEnum.From6pmTo9pm when @for.Hour >= 18 && @for.Hour < 21 => true,
                TerminalTransmissionScheduleEnum.From9pmTo12am when @for.Hour >= 21 && @for.Hour < 24 => true,
                _ => false
            };
    }
}
