using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace PoalimOnlineBusiness
{
    public class MasavData
    {
        private readonly List<TransactionRow> rows = new List<TransactionRow>();

        public MasavData(Header header, IEnumerable<TransactionRow> transactions, Footer footer)
        {
            this.Header = header;
            this.rows.AddRange(transactions); 
            this.Footer = footer;
        }

        public Header Header { get; }

        public IEnumerable<TransactionRow> Transactions => this.rows;

        public Footer Footer { get; }
    }
}
