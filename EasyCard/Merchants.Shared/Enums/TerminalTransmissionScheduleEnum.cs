using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Merchants.Shared.Enums
{
    public enum TerminalTransmissionScheduleEnum : short
    {
        [EnumMember(Value = "notSpecified")]
        NotSpecified = 0,

        [EnumMember(Value = "from12amTo3am")]
        From12amTo3am,

        [EnumMember(Value = "from3amTo6am")]
        From3amTo6am,

        [EnumMember(Value = "from6amTo9am")]
        From6amTo9am,

        [EnumMember(Value = "from9amTo12pm")]
        From9amTo12pm,

        [EnumMember(Value = "from12pmTo3pm")]
        From12pmTo3pm,

        [EnumMember(Value = "from3pmTo6pm")]
        From3pmTo6pm,

        [EnumMember(Value = "from6pmTo9pm")]
        From6pmTo9pm,

        [EnumMember(Value = "from9pmTo12am")]
        From9pmTo12am,
    }
}
