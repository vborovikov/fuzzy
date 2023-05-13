namespace FuzzyCompare.Text.Scanners
{
    using System;
    using System.Globalization;

    readonly struct NumberScanner : ITokenScanner
    {
        public TokenCategory Category => TokenCategory.Number;

        public bool Test(ReadOnlySpan<char> span, int start, CultureInfo culture)
        {
            return
                char.IsDigit(span[start]) ||
                (span.Length > 1 && span[start] == '-' && char.IsDigit(span[start + 1]) &&
                    (span[start + 1] != '0' || span.Length < 2 || (span[start + 2] != 'x' && span[start + 2] != 'X')));
        }

        public int Scan(ReadOnlySpan<char> span, int start, CultureInfo culture)
        {
            //todo: detect integers, floating-point numbers (all notations), hex numbers

            if (span[start] == '-')
                ++start;

            var hex = false;
            if (span[start] == '0')
            {
                ++start;
                hex = start < span.Length && (span[start] == 'x' || span[start] == 'X');
                if (hex)
                    ++start;
            }

            for (var i = start; i != span.Length; ++i)
            {
                if (hex && char.IsAsciiHexDigit(span[i]))
                    continue;

                if (!char.IsDigit(span[i]))
                {
                    return i;
                }
            }

            return span.Length;
        }
    }
}
