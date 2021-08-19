using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers
{
    public static class TransactionsHelper
    {
        public static Guid? GetJ5transactionID(Guid transactionID, int jdealType)
        {
            if (jdealType == 2)
            {
                return transactionID;
            }

            return null;
        }
    }
}
