using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models.PaymentDetails
{
    public class CashDetails : PaymentDetails
    {
        public CashDetails()
        {
            PaymentType = PaymentTypeEnum.Cash;
        }
    }
}
