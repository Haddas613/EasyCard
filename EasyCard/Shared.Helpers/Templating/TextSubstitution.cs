using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.Templating
{
    public class TextSubstitution
    {
        public TextSubstitution()
        {
        }

        public TextSubstitution(string substitution, string value)
        {
            Substitution = substitution;
            Value = value;
        }

        public string Substitution { get; set; }

        public string Value { get; set; }
    }
}
