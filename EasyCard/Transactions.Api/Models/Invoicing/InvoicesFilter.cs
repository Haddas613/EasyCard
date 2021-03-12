using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Helpers;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions.Enums;

namespace Transactions.Api.Models.Invoicing
{
    public class InvoicesFilter : FilterBase
    {
        public Guid? TerminalID { get; set; }

        public string InvoiceNumber { get; set; }

        public Guid? InvoiceID { get; set; }

        public CurrencyEnum? Currency { get; set; }

        public QuickDateFilterTypeEnum? QuickDateFilter { get; set; }

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

    }
}
