using System;
using System.ComponentModel.DataAnnotations;

namespace MerchantProfileApi.Models.Terminal
{
    public class Enable3DSRequest
    {
        public Guid TerminalID { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string ConsentAgreeText { get; set; }
    }
}
