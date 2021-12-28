using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Models
{
    /// <summary>
    /// Lookup element summary
    /// </summary>
    /// <typeparam name="T">Entity key type</typeparam>
    public class DictionarySummary<T>
    {
        public DictionarySummary()
        {
        }

        public DictionarySummary(T code, string description)
        {
            Code = code;
            Description = description;
        }

        public T Code { get; set; }

        public string Description { get; set; }
    }
}
