using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.Models.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ExcelFormatAttribute : Attribute
    {
        public ExcelFormatAttribute(string format)
        {
            Format = format;
        }

        public string Format { get; set; }
    }
}
