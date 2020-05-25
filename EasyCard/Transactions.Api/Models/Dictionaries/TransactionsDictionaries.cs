using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Dictionaries
{
    public class TransactionsDictionaries
    {
        public Dictionary<string, string> TransactionStatusEnum { get; set; }

        public Dictionary<string, string> TransactionTypeEnum { get; set; }

        public Dictionary<string, string> SpecialTransactionTypeEnum { get; set; }

        public Dictionary<string, string> JDealTypeEnum { get; set; }

        public Dictionary<string, string> RejectionReasonEnum { get; set; }

        public Dictionary<string, string> CurrencyEnum { get; set; }

        public Dictionary<string, string> CardPresenceEnum { get; set; }

        public Dictionary<string, string> QuickTimeFilterTypeEnum { get; set; }

        public Dictionary<string, string> QuickStatusFilterTypeEnum { get; set; }

        public Dictionary<string, string> DateFilterTypeEnum { get; set; }
    }
}
