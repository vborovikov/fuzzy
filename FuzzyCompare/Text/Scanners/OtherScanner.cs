namespace FuzzyCompare.Text.Scanners;

using System;
using System.Globalization;

readonly struct OtherScanner : ITokenScanner
{
	public TokenCategory Category => TokenCategory.Other;

	public bool Test(ReadOnlySpan<char> span, int start, CultureInfo culture)
	{
		return true;
	}

	public int Scan(ReadOnlySpan<char> span, int start, CultureInfo culture)
	{
		return start + 1;
	}
}
