namespace FuzzyCompare.Text.Scanners;

using System;
using System.Globalization;

readonly struct SymbolScanner : ITokenScanner
{
	public TokenCategory Category => TokenCategory.Symbol;

	public bool Test(ReadOnlySpan<char> span, int start, CultureInfo culture)
	{
		return Char.IsSymbol(span[start]);
	}

	public int Scan(ReadOnlySpan<char> span, int start, CultureInfo culture)
	{
		return start + 1;
	}
}
