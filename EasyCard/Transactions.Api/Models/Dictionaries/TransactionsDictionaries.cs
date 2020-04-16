using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Dictionaries
{
    public class TransactionsDictionaries
    {
        public IEnumerable<DictionarySummary<string>> TransactionStatuses { get; set; }

        public IEnumerable<DictionarySummary<string>> TransactionTypes { get; set; }

        public IEnumerable<DictionarySummary<string>> SpecialTransactionTypes { get; set; }

        public IEnumerable<DictionarySummary<short>> JDealTypes { get; set; }

        public IEnumerable<DictionarySummary<short>> RejectionReasons { get; set; }

        public IEnumerable<DictionarySummary<string>> Currencies { get; set; }

        public IEnumerable<DictionarySummary<string>> CardPresences { get; set; }

        public IEnumerable<DictionarySummary<string>> FilterTransactionQuickTime { get; set; }

        public IEnumerable<DictionarySummary<string>> FilterTransactionQuickStatuses { get; set; }

        public IEnumerable<DictionarySummary<string>> FilterDateTypes { get; set; }
    }
}
