using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PoalimOnlineBusiness.Contract
{
    public class ExportPacket
    {
        private readonly List<Transaction> _rows = new List<Transaction>();

        public ExportPacket(string instituteNumber, DateTime? paymentDate, DateTime? creationDate, string sendingInstitute, string institueName, string currency)
        {
            this.InstituteNumber = instituteNumber;
            this.PaymentDate = paymentDate ?? DateTime.UtcNow.Date;
            this.CreationDate = creationDate ?? DateTime.UtcNow.Date;
            this.SendingInstitute = sendingInstitute;
            this.InstitueName = institueName;
            this.Currency = currency;
        }

        public string InstituteNumber { get; private set; }

        public string Currency { get; private set; }

        public DateTime PaymentDate { get; private set; }

        public DateTime CreationDate { get; private set; }

        public string SendingInstitute { get; private set; }

        public string InstitueName { get; set; }

        public IEnumerable<Transaction> Rows => this._rows;

        public void AddRow(Transaction transaction)
        {
            this._rows.Add(transaction);
        }

        public void AddRows(IEnumerable<Transaction> transactions)
        {
            this._rows.AddRange(transactions);
        }

        public string GenerateExportFile()
        {
            var instituteNumber = Convert.ToInt32(this.InstituteNumber);
            var sendingInstitute = Convert.ToInt32(this.SendingInstitute);

            var header = new Header(instituteNumber, this.PaymentDate, this.CreationDate, sendingInstitute,this.InstitueName.ContainsHebrew()? string.Empty: this.InstitueName);



            var exportTransactions = new List<TransactionRow>();

            foreach (var transaction in this._rows)
            {
                var exportTransaction = new TransactionRow(instituteNumber, Convert.ToInt32(transaction.Bankcode),
                    Convert.ToInt32(transaction.BranchNumber), Convert.ToInt32(transaction.AccountNumber),this.InstitueName.ContainsHebrew()?  string.Empty : this.InstitueName,
                    transaction.Amount, transaction.Reference);

                exportTransactions.Add(exportTransaction);
            }

            var totalAmount = exportTransactions.Sum(r => r.Amount);
            var transactionsCount = exportTransactions.Count; // TODO: check limit of 55 records

            var footer = new Footer(instituteNumber, this.PaymentDate, totalAmount, transactionsCount);

            var export = new MasavData(header, exportTransactions, footer);

            return export.ExportString();
        }

    }
}
