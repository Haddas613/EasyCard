using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.UI
{
    public class MetadataOptionsAttribute : Attribute
    {
        public bool Hidden { get; set; }

        public int Order { get; set; }
    }
}
