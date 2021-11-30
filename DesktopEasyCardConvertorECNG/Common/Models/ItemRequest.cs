using Common.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Common.Models
{
    public class ItemRequest
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(128, MinimumLength = 3)]
        public string ItemName { get; set; }

        public decimal Price { get; set; }

        public bool Active { get; set; } = true;

        public CurrencyEnum Currency { get; set; }

        [StringLength(50)]
        public string ExternalReference { get; set; }

        public string BillingDesktopRefNumber { get; set; }
    }
}
