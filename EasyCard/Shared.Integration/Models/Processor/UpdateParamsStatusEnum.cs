using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public enum UpdateParamsStatusEnum : short
    {
        NotFoundOrInvalidStatus = 0,
        Updated = 1,
        UpdateFailed = -1
    }
}
