using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Helpers;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Shared.Enums;

namespace Transactions.Api.Models.Invoicing
{
    public class InvoicesFilter : FilterBase
    {
        public Guid? TerminalID { get; set; }

        public string InvoiceNumber { get; set; }

        public Guid? InvoiceID { get; set; }

        public Guid? BillingDealID { get; set; }

        public CurrencyEnum? Currency { get; set; }

        public QuickDateFilterTypeEnum? QuickDateFilter { get; set; }

        public InvoiceStatusEnum? Status { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }

        public InvoiceTypeEnum? InvoiceType { get; set; }

        public Guid? ConsumerID { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }

        public decimal InvoiceAmount { get; set; }

        public string ConsumerExternalReference { get; set; }
    }
}
