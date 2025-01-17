﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Terminal
{
    public class TerminalPaymentRequestSettingsUpdate
    {
        [StringLength(100)]
        public string FromAddress { get; set; }

        [StringLength(15)]
        [DataType(DataType.PhoneNumber)]
        public string FromPhoneNumber { get; set; }

        [StringLength(250)]
        public string DefaultRequestSubject { get; set; }

        [StringLength(250)]
        public string DefaultRefundRequestSubject { get; set; }

        [StringLength(50)]
        public string EmailTemplateCode { get; set; }
    }
}
