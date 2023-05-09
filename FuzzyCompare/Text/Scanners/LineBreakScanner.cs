namespace FuzzyCompare.Text.Scanners;

using System;
using System.Globalization;

readonly struct LineBreakScanner : ITokenScanner
{
    public TokenCategory Category { get; }

    public bool Test(ReadOnlySpan<char> span, int start, CultureInfo culture)
    {
        return span[start] == '\r' || span[start] == '\n';
    }

    public int Scan(ReadOnlySpan<char> span, int start, CultureInfo culture)
    {
        for (var i = start; i != span.Length; ++i)
        {
            if (Test(span, i, culture) == false)
            {
                return i;
            }
        }

        return span.Length;
    }
}
