using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.MDBSource
{
    public class Customer
    {
        public string Street { get; set; }
        public string BankAccount { get; set; }
        public int BankBranch { get; set; }

        public int BankID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string BillingTypeID { get; set; }

        /// <summary>
        /// CityName
        /// </summary>
        public string CityID { get; set; }
        public string ClientCode { get; set; }
        public bool Active { get; set; }
        public string FreeText { get; set; }
        public int BillingDay { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string ZipCode { get; set; }
        /// <summary>
        /// 0 credit, 1 masav, 2 invoice 
        /// </summary>
        public string PayType { get; set; }
        public string CardNumber { get; set; }
        public string CardDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? finishDate { get; set; }
        public string RivCode { get; set; }
        public string DealID { get; set; }
        public string RivName { get; set; }
    }
}
