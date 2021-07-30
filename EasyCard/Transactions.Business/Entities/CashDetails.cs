using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class CashDetails : PaymentDetails
    {
        public CashDetails()
        {
            PaymentType = Shared.Enums.PaymentTypeEnum.Cash;
        }
    }
}
