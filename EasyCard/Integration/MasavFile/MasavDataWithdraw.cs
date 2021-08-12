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
        private readonly List<TransactionRowWithdraw> rows = new List<TransactionRowWithdraw>();

        public MasavDataWithdraw(Header header, IEnumerable<TransactionRowWithdraw> transactions, Footer footer)
        {
            this.Header = header;
            this.rows.AddRange(transactions); 
            this.Footer = footer;
        }

        public Header Header { get; }

        public IEnumerable<TransactionRowWithdraw> Transactions => this.rows;

        public Footer Footer { get; }
    }
}
