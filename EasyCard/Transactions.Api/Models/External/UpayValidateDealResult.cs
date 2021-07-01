using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.External
{
    public class UpayValidateDealResult
    {
        public string Error { get; set; }

        public string CardType { get; set; }

        public string CardCompany { get; set; }

        public string IDNumber { get; set; }

        public string CardNumber { get; set; }

        public string ExpiryDate { get; set; }

        public string ForeignCard { get; set; }

        public string Amount { get; set; }

        public string CellPhoneNotify { get; set; }

        public string CardOwner { get; set; }

        public string CardReader { get; set; }

        public string ExternalID { get; set; }

        public string Solek { get; set; }

        public string NumPayments { get; set; }

        public string FirstPayment { get; set; }

        public string OtherPayment { get; set; }

        public string OkNumber { get; set; }

        public string ManpikID { get; set; }

        public string SixDigits { get; set; }

        public string CustomerConfirmation { get; set; }

        public string Terminal { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Error={0}&CardType={1}&CardCompany={2}&IDNumber={3}&CardNumber={4}&ExpiryDate ={5}&ForeignCard={6}&Amount={7}&CellPhoneNotify={8}&CardOwner={9}&CardReader={10}&ExternalID={11}&Solek={12}&NumPayments={13}&FirstPayment={14}&OtherPayment={15}&OkNumber={16}&ManpikID={17}&SixDigits={18}&CustomerConfirmation={19}&Terminal={20}",
                Error, CardType, CardCompany, IDNumber, CardNumber, ExpiryDate, ForeignCard, Amount, CellPhoneNotify, CardOwner, CardReader, ExternalID, Solek, NumPayments, FirstPayment, OtherPayment, OkNumber, ManpikID, SixDigits, CustomerConfirmation, Terminal);
        }
    }
}
