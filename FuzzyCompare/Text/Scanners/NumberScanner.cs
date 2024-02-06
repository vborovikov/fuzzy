namespace FuzzyCompare.Text.Scanners
{
    using System;
    using System.Globalization;

    readonly struct NumberScanner : ITokenScanner
    {
        public TokenCategory Category => TokenCategory.Number;

        public bool Test(ReadOnlySpan<char> span, int start, CultureInfo culture)
        {
            if (char.IsDigit(span[start]))
                return true;

            if (span[start] == '-' || span[start] == '+')
            {
                ++start;
                if (span.Length > start && char.IsDigit(span[start]))
                {
                    // check for hex number
                    if (span[start] == '0')
                    {
                        ++start;
                        if (span.Length > start && (span[start] == 'x' || span[start] == 'X'))
                        {
                            // not a number, treat the sign as a separate token
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public int Scan(ReadOnlySpan<char> span, int start, CultureInfo culture)
        {
            //todo: detect integers, floating-point numbers (all notations), hex numbers

            if (span[start] == '-' || span[start] == '+')
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
