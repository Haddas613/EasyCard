using System;
using System.ComponentModel.DataAnnotations;

namespace Transactions.Api.Models.Invoicing
{
    public class ResendSingleInvoiceRequest
    {
        /// <summary>
        /// Invoice ID
        /// </summary>
        public Guid InvoiceID { get; set; }

        /// <summary>
        /// Required. New invoice consumer email.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
