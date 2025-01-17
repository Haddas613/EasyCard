﻿using Shared.Api.Models;
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

        public Dictionary<string, string> QuickDateFilterTypeEnum { get; set; }

        public Dictionary<string, string> QuickStatusFilterTypeEnum { get; set; }

        public Dictionary<string, string> DateFilterTypeEnum { get; set; }

        public Dictionary<string, string> InvoiceTypeEnum { get; set; }

        public Dictionary<string, string> RepeatPeriodTypeEnum { get; set; }

        public Dictionary<string, string> StartAtTypeEnum { get; set; }

        public Dictionary<string, string> EndAtTypeEnum { get; set; }

        public Dictionary<string, string> InvoiceStatusEnum { get; set; }


        public Dictionary<string, string> SolekEnum { get; set; }

        public Dictionary<string, string> CardVendorEnum { get; set; }

        public Dictionary<string, string> PaymentRequestStatusEnum { get; set; }

        public Dictionary<string, string> PayReqQuickStatusFilterTypeEnum { get; set; }

        public Dictionary<string, string> PaymentTypeEnum { get; set; }

        public Dictionary<string, string> ReportGranularityEnum { get; set; }

        public Dictionary<string, string> QuickDateFilterAltEnum { get; set; }

        public Dictionary<string, string> DocumentOriginEnum { get; set; }

        public Dictionary<string, string> TransactionFinalizationStatusEnum { get; set; }

        public Dictionary<string, string> PropertyPresenceEnum { get; set; }

        public Dictionary<string, string> InvoiceBillingTypeEnum { get; set; }
    }
}
