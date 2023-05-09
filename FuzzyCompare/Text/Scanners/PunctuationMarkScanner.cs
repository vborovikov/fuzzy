namespace FuzzyCompare.Text.Scanners;

using System;
using System.Globalization;

readonly struct PunctuationMarkScanner : ITokenScanner
{
	public TokenCategory Category => TokenCategory.PunctuationMark;

	public bool Test(ReadOnlySpan<char> span, int start, CultureInfo culture)
	{
		return Char.IsPunctuation(span[start]);
	}

	public int Scan(ReadOnlySpan<char> span, int start, CultureInfo culture)
	{
		return start + 1;
	}
}
