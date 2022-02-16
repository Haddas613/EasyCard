using System;

namespace CheckoutPortal.Models
{
    public class SavedTokenInfo
    {
        public Guid CreditCardTokenID { get; set; }

        public string Label { get; set; }

        public DateTime? Created { get; set; }
    }
}
