﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RapidOneInvoices.Models
{
    [DataContract]
    public class CashModel
    {
        [DataMember(Name = "value")]
        public decimal Value { get; set; }
    }
}
