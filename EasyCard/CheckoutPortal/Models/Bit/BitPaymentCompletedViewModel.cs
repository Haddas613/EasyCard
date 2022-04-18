using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CheckoutPortal.Models.Bit
{
    public class BitPaymentCompletedViewModel
    {
        public string Message { get; set; }

        [BindNever]
        public string ReturnURL { get; set; }
    }
}
