﻿using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    /// <summary>
    /// To be used only in Shva
    /// </summary>
    public class CreditCardSecureDetails : CreditCardDetailsBase
    {
        public string Cvv { get; set; }
    }
}
