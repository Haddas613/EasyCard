using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api
{
    public class Headers
    {
#pragma warning disable SA1310 // Field names should not contain underscore
        public const string API_VERSION_HEADER = "x-version";

        public const string UI_VERSION_HEADER = "x-ui-version";
#pragma warning restore SA1310 // Field names should not contain underscore
    }
}
