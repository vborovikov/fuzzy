namespace FuzzyCompare.Text
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{Category}: {ToString()}")]
    public readonly struct Token : IEquatable<Token>, IEquatable<string>, IEquatable<char>
    {
        private readonly ReadOnlyMemory<char> source;
        private readonly Range range;

        internal Token(ReadOnlyMemory<char> source, Range range, TokenCategory category)
        {
            this.source = source;
            this.range = range;
            this.Category = category;
        }

        public ReadOnlySpan<char> Span => this.source[this.range].Span;
        public TokenCategory Category { get; }

        private int Length => this.range.GetOffsetAndLength(this.source.Length).Length;

        public static implicit operator ReadOnlySpan<char>(Token token) => token.Span;

        public static bool operator ==(Token a, Token b) => a.Equals(b);

        public static bool operator ==(Token a, string b) => a.Equals(b);

        public static bool operator ==(Token a, char b) => a.Equals(b);

        public static bool operator !=(Token a, Token b) => !(a == b);

        public static bool operator !=(Token a, string b) => !(a == b);

        public static bool operator !=(Token a, char b) => !(a == b);

        public override string ToString() => this.source[this.range].ToString();

        public override int GetHashCode()
        {
            return HashCode.Combine(this.source, this.range, this.Category);
        }

        public override bool Equals(object obj)
        {
            return obj switch
            {
                Token token => Equals(token),
                string str => Equals(str),
                char ch => Equals(ch),
                _ => false,
            };
        }

        public bool Equals(Token other)
        {
            if (this.Category != other.Category || this.Length != other.Length)
                return false;

            return EqualsSpan(other.Span);
        }

        public bool Equals(string other)
        {
            if (other is null || this.Length != other.Length)
                return false;

            return EqualsSpan(other);
        }

        public bool Equals(char other)
        {
            if (this.Length != 1)
                return false;

            return Char.ToUpperInvariant(this.source.Span[this.range.Start]).Equals(Char.ToUpperInvariant(other));
        }

        private bool EqualsSpan(ReadOnlySpan<char> span)
        {
            return this.Span.Equals(span, StringComparison.OrdinalIgnoreCase);
        }
    }
}