namespace FuzzyCompare.Text.Scanners;

using System;
using System.Globalization;

readonly struct WordScanner : ITokenScanner
{
    public TokenCategory Category => TokenCategory.Word;

    public bool Test(ReadOnlySpan<char> span, int start, CultureInfo culture)
    {
        if (Char.IsLetter(span[start]))
        {
            return true;
        }

        var category = CharUnicodeInfo.GetUnicodeCategory(span[start]);
        return category == UnicodeCategory.NonSpacingMark || category == UnicodeCategory.Format;
    }

    public int Scan(ReadOnlySpan<char> span, int start, CultureInfo culture)
    {
        for (var i = start; i < span.Length; ++i)
        {
            if (Test(span, i, culture) == false)
            {
                var nextIndex = i + 1;
                if (span[i] == '\'' && nextIndex < span.Length && Test(span, nextIndex, culture))
                {
                    var nextNextIndex = nextIndex + 1;
                    if (nextNextIndex < span.Length && Test(span, nextNextIndex, culture) == false)
                        continue;
                }
                return i;
            }
        }

        return span.Length;
    }
}
