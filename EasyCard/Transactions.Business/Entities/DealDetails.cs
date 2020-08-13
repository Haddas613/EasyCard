using Newtonsoft.Json.Linq;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class DealDetails
    {
        /// <summary>
        /// Deal reference on merchant side
        /// </summary>
        public string DealReference { get; set; }

        /// <summary>
        /// Deal description
        /// </summary>
        public string DealDescription { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        public string ConsumerPhone { get; set; }

        /// <summary>
        /// Consumer ID
        /// </summary>
        public Guid? ConsumerID { get; set; }

        /// <summary>
        /// Deal Items
        /// </summary>
        public IEnumerable<Item> Items { get; set; }

        /// <summary>
        /// End-customer Address
        /// </summary>
        public Address CustomerAddress { get; set; }
    }
}
