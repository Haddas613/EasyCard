﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Shared.Enums
{
    public enum FeatureEnum : short
    {
        PreventDoubleTansactions = 1,
        Billing = 2,
        SmsNotification = 3,
        Checkout = 4,
        Api = 5,
        CreditCardTokens = 6,
        Chargebacks = 7,
    }
}
