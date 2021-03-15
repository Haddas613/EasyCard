using Newtonsoft.Json.Linq;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Transactions.Business.Entities
{
    public class DealDetails
    {
        /// <summary>
        /// Deal reference on merchant side
        /// </summary>
        [StringLength(50)]
        public string DealReference { get; set; }

        /// <summary>
        /// Deal description
        /// </summary>
        [StringLength(500)]
        public string DealDescription { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        [StringLength(50)]
        public string ConsumerEmail { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        [StringLength(20)]
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
