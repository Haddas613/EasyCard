using Shared.Api.Models.Enums;
using ShvaEMV;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shva.Models
{
    public class InitInputObjRequest
    {
        public string ExpDate_YYMM { get; set; }

        public string TransactionType { get; set; }

        public string Currency { get; set; }

        public string Code { get; set; }

        public string CardNum { get; set; }

        public string CreditTerms { get; set; }

        public string Amount { get; set; }

        public string Cvv2 { get; set; }

        public string AuthNum { get; set; }

        public string Id { get; set; }

        public ParamJEnum ParamJ { get; set; }

        public string NumOfPayment{ get; set; }

        public string FirstAmount { get; set; }

        public string NonFirstAmount { get; set; } 

        public InitDealResultModel InitDealM { get; set; }

        public bool IsNewInitDeal { get; set; }

    }
}
