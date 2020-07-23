using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Models
{
    public class GroupedSummariesResponse<T>
    {
        public string GroupTitle { get; set; }

        public object GroupValue { get; set; }

        public IEnumerable<T> Data { get; set; }
    }
}
