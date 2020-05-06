using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Models.Metadata
{
    public class TableMeta
    {
        public IDictionary<string, ColMeta> Columns { get; set; }
    }
}
