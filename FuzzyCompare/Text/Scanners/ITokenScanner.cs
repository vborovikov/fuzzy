namespace FuzzyCompare.Text.Scanners;

using System;
using System.Globalization;

internal interface ITokenScanner
{
    TokenCategory Category { get; }

    bool Test(ReadOnlySpan<char> span, int start, CultureInfo culture);

    int Scan(ReadOnlySpan<char> span, int start, CultureInfo culture);
}