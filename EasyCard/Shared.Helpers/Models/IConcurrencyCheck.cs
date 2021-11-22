using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers
{
    public interface IConcurrencyCheck
    {
        byte[] UpdateTimestamp { get; set; }
    }
}
