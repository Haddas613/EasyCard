using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecwid.Models
{
    public class EcwidCart
    {
        public CurrencyEnum Currency { get; set; }

        public EcwidOrder Order { get; set; }
    }
}
