namespace FuzzyCompare.Text;

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using Scanners;

public static class Tokenizer
{
    public readonly ref struct TokenSpan
    {
        public TokenSpan(ReadOnlySpan<char> span, TokenCategory category)
        {
            this.Span = span;
            this.Category = category;
        }

        public ReadOnlySpan<char> Span { get; }
        public TokenCategory Category { get; }

        public void Deconstruct(out ReadOnlySpan<char> span, out TokenCategory category)
        {
            span = this.Span;
            category = this.Category;
        }

        public static implicit operator ReadOnlySpan<char>(TokenSpan token) => token.Span;
    }

    public ref struct TokenSpanEnumerator
    {
        private ReadOnlySpan<char> span;
        private readonly CultureInfo culture;

        internal TokenSpanEnumerator(ReadOnlySpan<char> span, CultureInfo culture)
        {
            this.span = span;
            this.culture = culture;
            this.Current = default;
        }

        public TokenSpan Current { get; private set; }

        public TokenSpanEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            var text = this.span;

            if (text.Length > 0)
            {
                var tokenCategory = Test(text, 0, this.culture);
                var tokenEndIndex = Scan(tokenCategory, text, 0, this.culture);
                if (tokenEndIndex < 0)
                    tokenEndIndex = text.Length;

                this.Current = new TokenSpan(text.Slice(0, tokenEndIndex), tokenCategory);
                this.span = text.Slice(tokenEndIndex);
                return true;
            }

            return false;
        }
    }

    public struct TokenEnumerator : IEnumerable<Token>, IEnumerator<Token>
    {
        private readonly ReadOnlyMemory<char> text;
        private readonly CultureInfo culture;
        private int start;
        private Token current;

        internal TokenEnumerator(ReadOnlyMemory<char> text, CultureInfo culture)
        {
            this.text = text;
            this.culture = culture;
        }

        public Token Current => this.current;

        public TokenEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            if (this.start >= this.text.Length)
                return false;

            var span = this.text.Span;
            var category = Test(span, this.start, this.culture);
            var end = Scan(category, span, this.start, this.culture);
            if (end < 0)
                end = this.text.Length;

            this.current = new Token(this.text, this.start..end, category);
            this.start = end;

            return true;
        }

        object? IEnumerator.Current => this.current;

        void IDisposable.Dispose()
        {
            // no-op
        }

        IEnumerator IEnumerable.GetEnumerator() => this;

        IEnumerator<Token> IEnumerable<Token>.GetEnumerator() => this;

        void IEnumerator.Reset()
        {
            this.current = default;
            this.start = 0;
        }
    }

#pragma warning disable CS0649
    private static readonly LineBreakScanner lineBreak;
    private static readonly WhiteSpaceScanner whiteSpace;
    private static readonly WordScanner word;
    private static readonly NumberScanner number;
    private static readonly OtherNumberScanner otherNumber;
    private static readonly PunctuationMarkScanner punctuationMark;
    private static readonly SymbolScanner symbol;
    private static readonly OtherScanner other;
#pragma warning restore CS0649

    public static TokenSpanEnumerator Tokenize(this ReadOnlySpan<char> span, CultureInfo? culture = null)
    {
        return new(span, culture ?? CultureInfo.CurrentCulture);
    }

    public static TokenEnumerator EnumerateTokens(this string str, CultureInfo? culture = null)
    {
        return new(str.AsMemory(), culture ?? CultureInfo.CurrentCulture);
    }

    private static TokenCategory Test(ReadOnlySpan<char> span, int start, CultureInfo culture)
    {
        // the test order is important!

        if (lineBreak.Test(span, start, culture)) return lineBreak.Category;
        if (whiteSpace.Test(span, start, culture)) return whiteSpace.Category;
        if (word.Test(span, start, culture)) return word.Category;
        if (number.Test(span, start, culture)) return number.Category;
        if (otherNumber.Test(span, start, culture)) return otherNumber.Category;
        if (punctuationMark.Test(span, start, culture)) return punctuationMark.Category;
        if (symbol.Test(span, start, culture)) return symbol.Category;
        if (other.Test(span, start, culture)) return other.Category;

        return TokenCategory.Other;
    }

    private static int Scan(TokenCategory category, ReadOnlySpan<char> span, int start, CultureInfo culture)
    {
        return category switch
        {
            TokenCategory.Other => other.Scan(span, start, culture),
            TokenCategory.LineBreak => lineBreak.Scan(span, start, culture),
            TokenCategory.WhiteSpace => whiteSpace.Scan(span, start, culture),
            TokenCategory.Word => word.Scan(span, start, culture),
            TokenCategory.Number => number.Scan(span, start, culture),
            TokenCategory.OtherNumber => otherNumber.Scan(span, start, culture),
            TokenCategory.PunctuationMark => punctuationMark.Scan(span, start, culture),
            TokenCategory.Symbol => symbol.Scan(span, start, culture),
            _ => throw new NotImplementedException()
        };
    }
}