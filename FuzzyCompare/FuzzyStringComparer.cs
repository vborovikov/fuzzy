﻿namespace FuzzyCompare;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

public sealed class FuzzyStringComparer : IEqualityComparer<string>
{
    public static readonly FuzzyStringComparer Normal = new(0.80f);
    public static readonly FuzzyStringComparer AboveNormal = new(0.85f);
    public static readonly FuzzyStringComparer Strong = new(0.90f);
    public static readonly FuzzyStringComparer Strongest = new(0.95f);
    public static readonly FuzzyStringComparer NormalInvariant = new(0.80f, CultureInfo.InvariantCulture);
    public static readonly FuzzyStringComparer AboveNormalInvariant = new(0.85f, CultureInfo.InvariantCulture);
    public static readonly FuzzyStringComparer StrongInvariant = new(0.90f, CultureInfo.InvariantCulture);
    public static readonly FuzzyStringComparer StrongestInvariant = new(0.95f, CultureInfo.InvariantCulture);

    public FuzzyStringComparer() : this(0.80f, CultureInfo.CurrentCulture) { }

    private FuzzyStringComparer(float tolerance) : this(tolerance, CultureInfo.CurrentCulture) { }

    private FuzzyStringComparer(float tolerance, CultureInfo culture)
    {
        this.Tolerance = tolerance;
        this.Culture = culture;
    }

    public float Tolerance { get; }
    public CultureInfo Culture { get; }

    public bool Equals(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x is null or { Length: 0 } || y is null or { Length: 0})
            return false;

        return Equals((ReadOnlySpan<char>)x, (ReadOnlySpan<char>)y);
    }

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

    public int GetHashCode([DisallowNull] string obj)
    {
        var hashCode = new HashCode();

        var length = Math.Min(obj.Length, 4);
        for (var i = 0; i != length; ++i)
        {
            hashCode.Add(char.ToUpper(obj[i], this.Culture));
        }

        return hashCode.ToHashCode();
    }
}
