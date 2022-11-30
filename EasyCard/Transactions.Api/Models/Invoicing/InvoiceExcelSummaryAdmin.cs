using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.UI;
using Shared.Helpers;
using Shared.Helpers.Models.Attributes;
using Shared.Integration.Models.PaymentDetails;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Shared.Enums;
using Transactions.Shared.Enums.Resources;

namespace Transactions.Api.Models.Invoicing
{
    public class InvoiceExcelSummaryAdmin : InvoiceSummaryAdmin
    {
        public string PaymentType
        {
            get
            {
                return PaymentTypeResource.ResourceManager.GetString(PaymentDetails == null ? string.Empty : PaymentDetails?.Select(x => x.PaymentType).FirstOrDefault().ToString(), new CultureInfo("he"));
            }
        }

        [ExcelIgnore]
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }

    }
}
