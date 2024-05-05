namespace FuzzyCompare.Text;

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using Scanners;

/// <summary>
/// Provides methods for tokenizing a string or span of characters.
/// </summary>
public static class Tokenizer
{
    /// <summary>
    /// Represents a reference to a token that was generated during tokenization.
    /// </summary>
    [DebuggerDisplay("{Category,nq}: {Span}")]
    public readonly ref struct SpanToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpanToken"/> structure with the specified span and category.
        /// </summary>
        /// <param name="span">The span of characters that the token represents.</param>
        /// <param name="category">The category of the token.</param>
        public SpanToken(ReadOnlySpan<char> span, TokenCategory category)
        {
            this.Span = span;
            this.Category = category;
        }

        /// <summary>
        /// Gets the span of characters that the token represents.
        /// </summary>
        public ReadOnlySpan<char> Span { get; }
        /// <summary>
        /// Gets the category of the token.
        /// </summary>
        public TokenCategory Category { get; }

        /// <summary>
        /// Deconstructs the <see cref="SpanToken"/> instance into its constituent parts.
        /// </summary>
        /// <param name="span">The span of characters that the token represents.</param>
        /// <param name="category">The category of the token.</param>
        public void Deconstruct(out ReadOnlySpan<char> span, out TokenCategory category)
        {
            span = this.Span;
            category = this.Category;
        }

        /// <summary>
        /// Implicitly converts a <see cref="SpanToken"/> to a <see cref="ReadOnlySpan{T}"/> of <see cref="char"/>.
        /// </summary>
        /// <param name="token">The token to convert.</param>
        public static implicit operator ReadOnlySpan<char>(SpanToken token) => token.Span;
    }

    /// <summary>
    /// Provides an enumerator for iterating over token references in a span of characters.
    /// </summary>
    public ref struct SpanTokenEnumerator
    {
        private ReadOnlySpan<char> span;
        private readonly CultureInfo culture;
        private SpanToken current;

        internal SpanTokenEnumerator(ReadOnlySpan<char> span, CultureInfo culture)
        {
            this.span = span;
            this.culture = culture;
        }

        /// <summary>
        /// Gets the current token reference.
        /// </summary>
        public readonly SpanToken Current => this.current;

        /// <summary>
        /// Returns the current instance of the <see cref="SpanTokenEnumerator"/> object.
        /// </summary>
        /// <returns>The current instance of the <see cref="SpanTokenEnumerator"/> object.</returns>
        public readonly SpanTokenEnumerator GetEnumerator() => this;

        /// <summary>
        /// Advances the enumerator to the next token reference in the span.
        /// </summary>
        public bool MoveNext()
        {
            var remaining = this.span;
            if (remaining.IsEmpty)
                return false;

            var tokenCategory = Test(remaining, 0, this.culture);
            var tokenEndIndex = Scan(tokenCategory, remaining, 0, this.culture);
            if (tokenEndIndex < 0)
                tokenEndIndex = remaining.Length;

            this.current = new SpanToken(remaining[..tokenEndIndex], tokenCategory);
            this.span = remaining[tokenEndIndex..];
            return true;
        }
    }

    /// <summary>
    /// Provides an enumerator for iterating over tokens in a string.
    /// </summary>
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

        /// <summary>
        /// Gets the current token.
        /// </summary>
        public readonly Token Current => this.current;

        /// <summary>
        /// Returns the current instance of the <see cref="TokenEnumerator"/> object.
        /// </summary>
        /// <returns>The current instance of the <see cref="TokenEnumerator"/> object.</returns>
        public readonly TokenEnumerator GetEnumerator() => this;

        /// <summary>
        /// Advances the enumerator to the next token in the string.
        /// </summary>
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

        readonly object? IEnumerator.Current => this.current;

        readonly void IDisposable.Dispose()
        {
            // no-op
        }

        readonly IEnumerator IEnumerable.GetEnumerator() => this;

        readonly IEnumerator<Token> IEnumerable<Token>.GetEnumerator() => this;

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

    /// <summary>
    /// Tokenizes a string and returns an enumerator for iterating over token references.
    /// </summary>
    /// <param name="str">The string to tokenize.</param>
    /// <param name="culture">The culture to use for string comparison.</param>
    /// <returns>An enumerator for iterating over token references.</returns>
    public static SpanTokenEnumerator EnumerateTokens(this string str, CultureInfo? culture = null) =>
        EnumerateTokens(str.AsSpan(), culture);

    /// <summary>
    /// Tokenizes a span of characters and returns an enumerator for iterating over token references.
    /// </summary>
    /// <param name="span">The span of characters to tokenize.</param>
    /// <param name="culture">The culture to use for string comparison.</param>
    /// <returns>An enumerator for iterating over token references.</returns>
    public static SpanTokenEnumerator EnumerateTokens(this ReadOnlySpan<char> span, CultureInfo? culture = null)
    {
        return new(span, culture ?? CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Tokenizes a string and returns an enumerator for iterating over tokens.
    /// </summary>
    /// <param name="str">The string to tokenize.</param>
    /// <param name="culture">The culture to use for string comparison.</param>
    /// <returns>An enumerator for iterating over tokens.</returns>
    public static TokenEnumerator Tokenize(this string str, CultureInfo? culture = null) =>
        Tokenize(str.AsMemory(), culture);

    /// <summary>
    /// Tokenizes a memory region of characters and returns an enumerator for iterating over tokens.
    /// </summary>
    /// <param name="memory">The memory region of characters to tokenize.</param>
    /// <param name="culture">The culture to use for string comparison.</param>
    /// <returns>An enumerator for iterating over tokens.</returns>
    public static TokenEnumerator Tokenize(this ReadOnlyMemory<char> memory, CultureInfo? culture = null)
    {
        return new(memory, culture ?? CultureInfo.CurrentCulture);
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
            _ => throw new NotSupportedException()
        };
    }
}