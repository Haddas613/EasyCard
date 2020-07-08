using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public enum TransmissionStatusEnum : short
    {
        NotFoundOrInvalidStatus = 0,
        Transmitted = 1,
        TransmissionFailed = -1
    }
}
