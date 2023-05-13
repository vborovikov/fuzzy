namespace FuzzyCompare.Text.Scanners;

using System;
using System.Globalization;

/// <summary>
/// Defines the interface for a token scanner that can be used by the tokenizer.
/// </summary>
internal interface ITokenScanner
{
    /// <summary>
    /// Gets the category of the tokens that the scanner generates.
    /// </summary>
    TokenCategory Category { get; }

    /// <summary>
    /// Tests whether a span of characters starting at the specified position matches
    /// the scan pattern for the scanner.
    /// </summary>
    /// <param name="span">The span of characters to test.</param>
    /// <param name="start">The starting position of the span.</param>
    /// <param name="culture">The culture to use for string comparison.</param>
    /// <returns>True if the span matches the scan pattern, false otherwise.</returns>
    bool Test(ReadOnlySpan<char> span, int start, CultureInfo culture);

    /// <summary>
    /// Scans a span of characters starting at the specified position 
    /// and generates a token if the scan pattern matches.
    /// </summary>
    /// <param name="span">The span of characters to scan.</param>
    /// <param name="start">The starting position of the span.</param>
    /// <param name="culture">The culture to use for string comparison.</param>
    /// <returns>The position of the end of the token if a token was generated, otherwise the span length.</returns>
    int Scan(ReadOnlySpan<char> span, int start, CultureInfo culture);
}