﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RapidOneInvoices.Models
{
    [DataContract]
    public class CheckModel
    {
        [DataMember(Name = "value")]
        public decimal Value { get; set; }
    }
}
