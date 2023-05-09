namespace FuzzyCompare.Text.Scanners;

using System;
using System.Globalization;

readonly struct WhiteSpaceScanner : ITokenScanner
{
	public TokenCategory Category => TokenCategory.WhiteSpace;

	public bool Test(ReadOnlySpan<char> span, int start, CultureInfo culture)
	{
		return Char.IsWhiteSpace(span[start]);
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
