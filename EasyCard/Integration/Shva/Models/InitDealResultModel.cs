using System;
using System.Collections.Generic;
using System.Text;

namespace Shva.Models
{
    public class InitDealResultModel
    {
        public string OriginalUid { get; set; }
        public string OriginalTranDate { get; set; }
        public string OriginalTranTime { get; set; }
        public string Amount { get; set; }
        public string OriginalAuthNum { get; set; }//מנפיק
        public string OriginalAuthSolekNum { get; set; }
        // public string OriginalAuthNumpikNum { get; set; }
        public string OriginalAuthorizationCodeManpik { get; set; }
        public string OriginalAuthorizationCodeSolek { get; set; }
        public int DealsCounter { get; set; }
        public bool IsEmpty()
        {
            foreach (var p in this.GetType().GetProperties())
            {
                var v = p.GetValue(this);
                int res = 0;
                if (v == null || string.IsNullOrWhiteSpace(v.ToString()) || (v != null && !string.IsNullOrWhiteSpace(v.ToString()) && int.TryParse(v.ToString(), out res) && !(res > 0)))
                {
                    continue;
                }

                return false;
            }
            return true;
        }
    }
}
