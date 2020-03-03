using Shared.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    // TODO: fill required fields
    public class ExternalPaymentTransactionRequest
    {
        public Object ProcessorSettings { get; set; }
       

        //public clsInput ClsInput { get; set; }
        public decimal Amount { get; set; }

        public string CreditCardNumber { get; set; }
        public string CVV { get; set; }

        public string Urack2 { get; set; }

        public string ExpDate_YYMM { get; set; }
        /// <summary>
        /// 11 initialization/after initialization transaction
        /// 01 regular deal
        /// 53 refund
        /// </summary>
        public string TransactionType { get; set; }
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
        public ParamJEnum ParamJ { get; set; }

    }
}
