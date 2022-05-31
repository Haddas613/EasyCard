using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Billing
{
    public class ConsumerSummary
    {
        [Shared.Api.UI.MetadataOptions(Hidden = true)]
        public Guid ConsumerID { get; set; }

        [Shared.Api.UI.MetadataOptions(Hidden = true)]
        public Guid TerminalID { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        [Shared.Api.UI.MetadataOptions(Order = 4)]
        public string ConsumerEmail { get; set; }

        [Shared.Api.UI.MetadataOptions(Order = 1)]
        public string ConsumerName { get; set; }

        [Shared.Api.UI.MetadataOptions(Order = 3)]
        public string ConsumerPhone { get; set; }

        [Shared.Api.UI.MetadataOptions(Order = 2)]
        public string ConsumerNationalID { get; set; }

        [Shared.Api.UI.MetadataOptions(Hidden = true)]
        public Address ConsumerAddress { get; set; }

        [Shared.Api.UI.MetadataOptions(Order = 5)]
        public string ExternalReference { get; set; }

        /// <summary>
        /// External ID inside https://woocommerce.com system
        /// </summary>
        [Shared.Api.UI.MetadataOptions(Hidden = true)]
        public string WoocommerceID { get; set; }

        /// <summary>
        /// External ID inside https://www.ecwid.com system
        /// </summary>
        [Shared.Api.UI.MetadataOptions(Hidden = true)]
        public string EcwidID { get; set; }

        public bool Active { get; set; }
    }
}
