using Merchants.Business.Entities.Terminal;
using Merchants.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Extensions
{
    public static class TerminalExtensions
    {
        public static bool FeatureEnabled(this Terminal terminal, FeatureEnum feature)
            => terminal.EnabledFeatures?.Contains(feature) == true;
    }
}
