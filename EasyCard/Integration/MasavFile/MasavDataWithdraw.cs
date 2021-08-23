using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace PoalimOnlineBusiness
{
    public class MasavDataWithdraw
    {
        public Header Header { get; set; }

        public IEnumerable<TransactionRowWithdraw> Transactions { get; set; }

        public Footer Footer { get; set; }
    }
}
