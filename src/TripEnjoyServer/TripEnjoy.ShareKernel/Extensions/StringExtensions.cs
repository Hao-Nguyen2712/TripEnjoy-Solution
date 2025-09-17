using System.Globalization;
using System.Linq;

namespace TripEnjoy.ShareKernel.Extensions;

public static class StringExtensions
{
    public static string ToTitleCase(this string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return s;

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
    }
}
