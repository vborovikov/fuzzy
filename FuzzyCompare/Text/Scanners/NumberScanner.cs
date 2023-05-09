namespace FuzzyCompare.Text.Scanners
{
	using System;
    using System.Globalization;

	readonly struct NumberScanner : ITokenScanner
	{
		public TokenCategory Category => TokenCategory.Number;

		public bool Test(ReadOnlySpan<char> span, int start, CultureInfo culture)
		{
			return Char.IsDigit(span[start]);
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
}
