using System;
using System.Collections.Generic;
using System.Text;

namespace RapidOne.Models
{
    public class RapidOneSummaryResponse<T>
    {
        public int Total { get; set; }

        public IEnumerable<T> Data { get; set; }
    }
}
