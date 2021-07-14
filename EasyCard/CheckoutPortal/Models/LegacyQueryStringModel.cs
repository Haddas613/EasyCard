using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models
{
    public class LegacyQueryStringModel
    {
        public string StateData { get; set; }
        public string CardOwner { get; set; }
        public string OwnerEmail { get; set; }
        public string Id { get; set; }//= { 0 }", paymentRequest guid
        public string OkNumber { get; set; }
        public string Code { get; set; } //shva code result
        public string DealID { get; set; }// = Payment Transaction guid
        public string BusinessName { get; set; }
        public string Terminal { get; set; }
        public string DealNumber { get; set; }
        public string CardNumber { get; set; }
        public string DealDate { get; set; }
        public string PayNumber { get; set; }
        public string FirstPay { get; set; }
        public string AddPay { get; set; }
        public string DealTypeOut { get; set; }
        public string DealType { get; set; }
        public string Currency { get; set; }
        public string CardNameID { get; set; }
        public string Manpik { get; set; }
        public string Mutag { get; set; }
        public string DealTypeID { get; set; }
        public string CurrencyID { get; set; }
        public string CardNameIDCode { get; set; }
        public string ManpikID { get; set; }
        public string MutagID { get; set; }
        public string Tz { get; set; }
        public string CardDate { get; set; }
        public string Token { get; set; }
        public string phoneNumber { get; set; }
        public string EmvSoftVersion { get; set; }
        public string OriginalUID { get; set; }
        public string CompRetailerNum { get; set; }
    }
}
