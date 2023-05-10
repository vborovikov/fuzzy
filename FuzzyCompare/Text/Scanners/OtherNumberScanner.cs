namespace FuzzyCompare.Text.Scanners;

using System;
using System.Globalization;

readonly struct OtherNumberScanner : ITokenScanner
{
	public TokenCategory Category => TokenCategory.OtherNumber;

	public bool Test(ReadOnlySpan<char> span, int start, CultureInfo culture)
	{
		return Char.IsNumber(span[start]);
	}

	public int Scan(ReadOnlySpan<char> span, int start, CultureInfo culture)
	{
		//todo: detect fractional numbers, vulgars, virgule symbol, solidus symbol, sup- and sub- digits

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
