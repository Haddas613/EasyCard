using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models
{
    public class SummariesResponse<T>
    {
        public int NumberOfRecords { get; set; }

        public IEnumerable<T> Data { get; set; }
    }
}
