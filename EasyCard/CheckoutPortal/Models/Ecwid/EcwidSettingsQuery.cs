using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CheckoutPortal.Models.Ecwid
{
    public class EcwidSettingsQuery
    {
        [BindNever]
        public string AppID { get; set; }
    }
}
