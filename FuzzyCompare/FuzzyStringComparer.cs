namespace FuzzyCompare;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

/// <summary>
/// Provides a way to compare two strings for equality and generate hash codes for them.
/// </summary>
public interface IStringEqualityComparer : IEqualityComparer<string>
{
    /// <summary>
    /// Determines whether two read-only character spans are equal.
    /// </summary>
    /// <param name="x">The first read-only character span to compare.</param>
    /// <param name="y">The second read-only character span to compare.</param>
    /// <returns>true if the specified character spans are equal; otherwise, false.</returns>
    bool Equals(ReadOnlySpan<char> x, ReadOnlySpan<char> y);

    /// <summary>
    /// Returns a hash code for a read-only character span.
    /// </summary>
    /// <param name="obj">The read-only character span to generate a hash code for.</param>
    /// <returns>A hash code for the specified character span.</returns>
    int GetHashCode(ReadOnlySpan<char> obj);
}

/// <summary>
/// Compares two strings for fuzzy equality using the Jaro-Winkler distance algorithm.
/// </summary>
public sealed class FuzzyStringComparer : IStringEqualityComparer
{
    /// <summary>
    /// Gets a <see cref="FuzzyStringComparer"/> object that uses a tolerance of 0.80 and the current culture.
    /// </summary>
    public static readonly FuzzyStringComparer Normal = new(0.80f);
    /// <summary>
    /// Gets a <see cref="FuzzyStringComparer"/> object that uses a tolerance of 0.85 and the current culture.
    /// </summary>
    public static readonly FuzzyStringComparer AboveNormal = new(0.85f);
    /// <summary>
    /// Gets a <see cref="FuzzyStringComparer"/> object that uses a tolerance of 0.90 and the current culture.
    /// </summary>
    public static readonly FuzzyStringComparer Strong = new(0.90f);
    /// <summary>
    /// Gets a <see cref="FuzzyStringComparer"/> object that uses a tolerance of 0.95 and the current culture.
    /// </summary>
    public static readonly FuzzyStringComparer Strongest = new(0.95f);
    /// <summary>
    /// Gets a <see cref="FuzzyStringComparer"/> object that uses a tolerance of 0.80 and the <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public static readonly FuzzyStringComparer NormalInvariant = new(0.80f, CultureInfo.InvariantCulture);
    /// <summary>
    /// Gets a <see cref="FuzzyStringComparer"/> object that uses a tolerance of 0.85 and the <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public static readonly FuzzyStringComparer AboveNormalInvariant = new(0.85f, CultureInfo.InvariantCulture);
    /// <summary>
    /// Gets a <see cref="FuzzyStringComparer"/> object that uses a tolerance of 0.90 and the <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public static readonly FuzzyStringComparer StrongInvariant = new(0.90f, CultureInfo.InvariantCulture);
    /// <summary>
    /// Gets a <see cref="FuzzyStringComparer"/> object that uses a tolerance of 0.95 and the <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public static readonly FuzzyStringComparer StrongestInvariant = new(0.95f, CultureInfo.InvariantCulture);

    /// <summary>
    /// Initializes a new instance of the <see cref="FuzzyStringComparer"/> class with a default tolerance of 0.80 and the current culture.
    /// </summary>
    public FuzzyStringComparer() : this(0.80f, CultureInfo.CurrentCulture) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FuzzyStringComparer"/> class with the specified tolerance and the current culture.
    /// </summary>
    /// <param name="tolerance">The tolerance level for fuzzy comparison. Must be between 0 and 1.</param>
    public FuzzyStringComparer(float tolerance) : this(tolerance, CultureInfo.CurrentCulture) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FuzzyStringComparer"/> class with the specified tolerance and culture.
    /// </summary>
    /// <param name="tolerance">The tolerance level for fuzzy comparison. Must be between 0 and 1.</param>
    /// <param name="culture">The culture to use for case-insensitive comparison.</param>
    public FuzzyStringComparer(float tolerance, CultureInfo culture)
    {
        this.Tolerance = tolerance;
        this.Culture = culture;
    }

    /// <summary>
    /// Gets the tolerance level for fuzzy comparison.
    /// </summary>
    public float Tolerance { get; }

    /// <summary>
    /// Gets the culture to use for case-insensitive comparison.
    /// </summary>
    public CultureInfo Culture { get; }

    /// <inheritdoc/>
    public bool Equals(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x is null or { Length: 0 } || y is null or { Length: 0})
            return false;

        return Equals((ReadOnlySpan<char>)x, (ReadOnlySpan<char>)y);
    }

    /// <inheritdoc/>
    public bool Equals(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
    {
        if (x.IsEmpty || y.IsEmpty)
            return false;

        Span<char> xs = stackalloc char[x.Length];
        x.ToUpper(xs, this.Culture);
        Span<char> ys = stackalloc char[y.Length];
        y.ToUpper(ys, this.Culture);

        return ComparisonMethods.JaroWinklerSimilarity(xs, ys) >= this.Tolerance;
    }

    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] string obj)
    {
        return GetHashCode((ReadOnlySpan<char>)obj);
    }

    /// <inheritdoc/>
    public int GetHashCode(ReadOnlySpan<char> obj)
    {
        var length = Math.Min(obj.Length, 4);
        return this.Culture.CompareInfo.GetHashCode(obj[..length], CompareOptions.IgnoreCase);
    }
}
