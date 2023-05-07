namespace FuzzyCompare;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public sealed class StringArrayComparer : IEqualityComparer<string[]>
{
    private readonly IEqualityComparer<string> stringComparer;

    public StringArrayComparer(IEqualityComparer<string> stringComparer)
    {
        this.stringComparer = stringComparer;
    }

    public bool Equals(string[]? x, string[]? y) => Equals(x, y, this.stringComparer);

    public static bool Equals(string[]? x, string[]? y, IEqualityComparer<string> stringComparer)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }
        if (x is null || y is null)
        {
            return false;
        }
        if (x.Length != y.Length)
        {
            return false;
        }

        for (var i = 0; i != x.Length; ++i)
        {
            if (!stringComparer.Equals(x[i], y[i]))
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode([DisallowNull] string[] obj) => GetHashCode(obj, this.stringComparer);

    public static int GetHashCode([DisallowNull] string[] obj, IEqualityComparer<string> stringComparer)
    {
        var hashCode = new HashCode();
        foreach (var item in obj)
        {
            hashCode.Add(item, stringComparer);
        }
        return hashCode.ToHashCode();
    }
}