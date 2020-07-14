using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Business
{
    public interface IConcurrencyCheck
    {
        byte[] UpdateTimestamp { get; set; }
    }
}
