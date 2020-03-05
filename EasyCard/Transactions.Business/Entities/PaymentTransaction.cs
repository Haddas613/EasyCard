using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Shared.Enums;

namespace Transactions.Business.Entities
{
    public class PaymentTransaction
    {
        public long PaymentTransactionID { get; set; }

        /// <summary>
        /// Individual counter per terminal
        /// </summary>
        public long TransactionNumber { get; set; }

        public long TerminalID { get; set; }

        public long MerchantID { get; set; }

        public decimal Amount { get; set; }

        public string Urack2 { get; set; }

        public string CreditCardNumber { get; set; }

        public string ExpirationDate { get; set; }

        public TransactionTypeEnum TransactionType { get; set; }

        /// <summary>
        /// ///* "840";//USD   "978";//Euro   "376";//ILS*/
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 50 telephone deal
        /// 00 regular (megnetic)
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 1 regular
        /// 6 credit
        /// 8 intallmets
        /// </summary>
        public string CreditTerms { get; set; }

        /// <summary>
        /// ""  in case of regular deal
        /// </summary>
        public string NumOfInstallment { get; set; }

        /// <summary>
        /// ""  in case of regular deal
        /// </summary>
        public string FirstAmount { get; set; }

        /// <summary>
        /// "" in case of regular deal
        /// </summary>
        public string NonFirstAmount { get; set; }

        public string DealDescription { get; set; }

        public string IdentityNumber { get; set; }

        public DateTime Created { get; set; }

        public byte[] UpdateTimestamp { get; set; }
    }
}
