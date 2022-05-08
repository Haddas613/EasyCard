using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidOne.DTOs.PaymentIntegration
{
    public class ECNGConfig
    {
        public long PaymentSystemConfigId { get; set; }

        public string SecretKey { get; set; }

        public string SharedApiKey { get; set; }

        public Environment Environment { get; set; }

        public IEnumerable<CCVendorMapping> CCVendorMapping { get; set; }

        public IEnumerable<CCDealTypeMapping> CCDealTypeMapping { get; set; }

        public static int? GetSAPCCDealTypeId(string easyCardDealType, IEnumerable<CCDealTypeMapping> CCDealTypeMapping)
        {
            return CCDealTypeMapping?.Where(m => m.PaymentSystemDealType.Equals(easyCardDealType, StringComparison.InvariantCultureIgnoreCase)).Select(m => m.CCDealTypeSAPId).FirstOrDefault();
        }

        public static int? GetSAPCCVendorId(string stripeVendorName, IEnumerable<CCVendorMapping> CCVendorMapping)
        {
            return CCVendorMapping?.Where(m => m.PaymentSystemVendorName.Equals(stripeVendorName, StringComparison.InvariantCultureIgnoreCase)).Select(m => m.CCSAPId).FirstOrDefault();
        }
    }

    public enum Environment
    {
        QA = 0,
        STAGE = 1,
        LIVE = 2
    }
}
