using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.Templating
{
    public class TemplateProcessor
    {
        public static string Substitute(string text, IEnumerable<TextSubstitution> substitutions)
        {
            var sb = new StringBuilder(text);

            foreach (var s in substitutions)
            {
                sb.Replace($"{{{s.Substitution}}}", s.Value);
            }

            return sb.ToString();
        }
    }
}
