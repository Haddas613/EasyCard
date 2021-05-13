using System.Collections.Generic;
using System.Globalization;

public class CultureSwitcherModel
{
    public CultureInfo CurrentUICulture { get; set; }
    public List<CultureInfo> SupportedCultures { get; set; }
}
