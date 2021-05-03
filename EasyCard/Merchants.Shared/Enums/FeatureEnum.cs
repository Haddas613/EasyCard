using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Shared.Enums
{
    public enum FeatureEnum : short
    {
        PreventDoubleTansactions = 1,
        RecurrentPayments = 2,
        SmsNotification = 3,
        Checkout = 4,
        Api = 5,
        Billing = 6
    }
}
