namespace FuzzyCompare;

using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

/// <summary>
/// A static class that provides methods for comparing strings using fuzzy logic.
/// </summary>
public static class ComparisonMethods
{
    /// <summary>
    /// Calculates the Jaro similarity between two given strings using a fuzzy matching algorithm.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="target">The target string.</param>
    /// <returns>A float value between 0 and 1 representing the similarity between the two inputs.</returns>
    public static float JaroSimilarity(this ReadOnlySpan<char> source, ReadOnlySpan<char> target)
    {
        var sourceIntersectTarget = source.CommonChars(target);
        var targetIntersectSource = target.CommonChars(source);
        if (sourceIntersectTarget.IsEmpty || targetIntersectSource.IsEmpty)
            return 0f;
        if (sourceIntersectTarget.Length != targetIntersectSource.Length)
            return 0f;

        var length = sourceIntersectTarget.Length;
        var transpositions = 0;
        for (var i = 0; i != length; ++i)
        {
            if (sourceIntersectTarget[i] != targetIntersectSource[i])
                ++transpositions;
        }

        var t = transpositions / 2f;
        var m = (float)length;
        return ((m / source.Length) + (m / target.Length) + ((m - t) / m)) / 3f;
    }

    /// <summary>
    /// Calculates the Jaro-Winkler similarity between two given strings using a fuzzy matching algorithm.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="target">The target string.</param>
    /// <param name="p">A scaling factor for the weight of the common prefix. Defaults to 0.1.</param>
    /// <returns>A float value between 0 and 1 representing the similarity between the two inputs.</returns>
    public static float JaroWinklerSimilarity(this ReadOnlySpan<char> source, ReadOnlySpan<char> target, float p = 0.1f)
    {
        var prefixScale = p switch
        {
            > 0.25f => 0.25f, // for similarity to not exceed 1
            < 0f => 0f,       // jaro similarity
            _ => p
        };

        var jaroSimilarity = source.JaroSimilarity(target);
        var commonPrefixLength = source[..Math.Min(4, source.Length)].CommonPrefixLength(target[..Math.Min(4, target.Length)]);

        return jaroSimilarity + (commonPrefixLength * prefixScale * (1 - jaroSimilarity));
    }

    /// <summary>
    /// Calculates the Levenshtein distance between two given strings using a fuzzy matching algorithm.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="target">The target string.</param>
    /// <returns>An integer representing the edit distance between the two inputs.</returns>
    public static unsafe int LevenshteinDistance(this ReadOnlySpan<char> source, ReadOnlySpan<char> target)
    {
        var startIndex = 0;
        var sourceEnd = source.Length;
        var targetEnd = target.Length;

        fixed (char* sourcePtr = source)
        fixed (char* targetPtr = target)
        {
            var charactersAvailableToTrim = Math.Min(targetEnd, sourceEnd);

            if (Avx2.IsSupported)
            {
                var sourceUShortPtr = (ushort*)sourcePtr;
                var targetUShortPtr = (ushort*)targetPtr;

                while (charactersAvailableToTrim >= Vector256<ushort>.Count)
                {
                    var match = (uint)Avx2.MoveMask(
                        Avx2.CompareEqual(
                            Avx.LoadDquVector256(sourceUShortPtr + startIndex),
                            Avx.LoadDquVector256(targetUShortPtr + startIndex)
                        ).AsByte()
                    );

                    if (match != uint.MaxValue)
                    {
                        var remaining = BitOperations.TrailingZeroCount(match ^ uint.MaxValue) / sizeof(ushort);
                        startIndex += remaining;
                        charactersAvailableToTrim -= remaining;
                        break;
                    }

                    startIndex += Vector256<ushort>.Count;
                    charactersAvailableToTrim -= Vector256<ushort>.Count;
                }

                while (charactersAvailableToTrim >= Vector256<ushort>.Count)
                {
                    var match = (uint)Avx2.MoveMask(
                        Avx2.CompareEqual(
                            Avx.LoadDquVector256(sourceUShortPtr + sourceEnd - Vector256<ushort>.Count),
                            Avx.LoadDquVector256(targetUShortPtr + targetEnd - Vector256<ushort>.Count)
                        ).AsByte()
                    );

                    if (match != uint.MaxValue)
                    {
                        var lastMatch = BitOperations.LeadingZeroCount(match ^ uint.MaxValue) / sizeof(ushort);
                        sourceEnd -= lastMatch;
                        targetEnd -= lastMatch;
                        break;
                    }

                    sourceEnd -= Vector256<ushort>.Count;
                    targetEnd -= Vector256<ushort>.Count;
                    charactersAvailableToTrim -= Vector256<ushort>.Count;
                }
            }

            while (charactersAvailableToTrim > 0 && source[startIndex] == target[startIndex])
            {
                charactersAvailableToTrim--;
                startIndex++;
            }

            while (charactersAvailableToTrim > 0 && source[sourceEnd - 1] == target[targetEnd - 1])
            {
                charactersAvailableToTrim--;
                sourceEnd--;
                targetEnd--;
            }
        }

        var sourceLength = sourceEnd - startIndex;
        var targetLength = targetEnd - startIndex;

        if (sourceLength == 0)
        {
            return targetLength;
        }

        if (targetLength == 0)
        {
            return sourceLength;
        }

        var sourceSpan = source.Slice(startIndex, sourceLength);
        var targetSpan = target.Slice(startIndex, targetLength);

        fixed (char* sourcePtr = sourceSpan)
        fixed (char* targetPtr = targetSpan)
        {
            if (Sse41.IsSupported)
            {
                var diag1Array = ArrayPool<int>.Shared.Rent(sourceLength + 1);
                var diag2Array = ArrayPool<int>.Shared.Rent(sourceLength + 1);

                fixed (int* diag1Ptr = diag1Array)
                fixed (int* diag2Ptr = diag2Array)
                {
                    var localDiag1Ptr = diag1Ptr;
                    var localDiag2Ptr = diag2Ptr;
                    int rowIndex, columnIndex, endRow;
                    new Span<int>(diag1Ptr, sourceLength + 1).Clear();
                    new Span<int>(diag2Ptr, sourceLength + 1).Clear();

                    var counter = 1;
                    while (true)
                    {
                        var startRow = counter > targetLength ? counter - targetLength : 1;

                        if (counter > sourceLength)
                        {
                            endRow = sourceLength;
                        }
                        else
                        {
                            Unsafe.Write(Unsafe.Add<int>(localDiag1Ptr, counter), counter);
                            endRow = counter - 1;
                        }

                        for (rowIndex = endRow; rowIndex >= startRow;)
                        {
                            columnIndex = counter - rowIndex;
                            if (rowIndex >= Vector128<int>.Count && targetLength - columnIndex >= Vector128<int>.Count)
                            {
                                var sourceVector = Sse41.ConvertToVector128Int32((ushort*)sourcePtr + rowIndex - Vector128<int>.Count);
                                var targetVector = Sse41.ConvertToVector128Int32((ushort*)targetPtr + columnIndex - 1);
                                targetVector = Sse2.Shuffle(targetVector, 0x1b);
                                var substitutionCostAdjustment = Sse2.CompareEqual(sourceVector, targetVector);

                                var substitutionCost = Sse2.Add(
                                    Sse3.LoadDquVector128(localDiag1Ptr + rowIndex - Vector128<int>.Count),
                                    substitutionCostAdjustment
                                );

                                var deleteCost = Sse3.LoadDquVector128(localDiag2Ptr + rowIndex - (Vector128<int>.Count - 1));
                                var insertCost = Sse3.LoadDquVector128(localDiag2Ptr + rowIndex - Vector128<int>.Count);

                                var localCost = Sse41.Min(Sse41.Min(insertCost, deleteCost), substitutionCost);
                                localCost = Sse2.Add(localCost, Vector128.Create(1));

                                Sse2.Store(localDiag1Ptr + rowIndex - (Vector128<int>.Count - 1), localCost);
                                rowIndex -= Vector128<int>.Count;
                            }
                            else
                            {
                                var localCost = Math.Min(localDiag2Ptr[rowIndex], localDiag2Ptr[rowIndex - 1]);
                                if (localCost < diag1Ptr[rowIndex - 1])
                                {
                                    localDiag1Ptr[rowIndex] = localCost + 1;
                                }
                                else
                                {
                                    localDiag1Ptr[rowIndex] = localDiag1Ptr[rowIndex - 1] + (sourcePtr[rowIndex - 1] != targetPtr[columnIndex - 1] ? 1 : 0);
                                }
                                rowIndex--;
                            }
                        }

                        if (counter == sourceLength + targetLength)
                        {
                            var result = Unsafe.Read<int>(Unsafe.Add<int>(localDiag1Ptr, startRow));
                            ArrayPool<int>.Shared.Return(diag1Array);
                            ArrayPool<int>.Shared.Return(diag2Array);
                            return result;
                        }

                        Unsafe.Write(localDiag1Ptr, counter);

                        var tempPtr = localDiag1Ptr;
                        localDiag1Ptr = localDiag2Ptr;
                        localDiag2Ptr = tempPtr;

                        counter++;
                    }
                }
            }
            else
            {
                var previousRow = ArrayPool<int>.Shared.Rent(targetSpan.Length);
                var allOnesVector = Vector128.Create(1);

                fixed (int* previousRowPtr = previousRow)
                {
                    for (var columnIndex = 0; columnIndex < targetLength; columnIndex++)
                    {
                        previousRowPtr[columnIndex] = columnIndex + 1;
                    }

                    for (var rowIndex = 0; rowIndex < sourceLength; rowIndex++)
                    {
                        var lastSubstitutionCost = rowIndex;
                        var lastInsertionCost = rowIndex + 1;

                        var sourceChar = sourcePtr[rowIndex];

                        for (var columnIndex = 0; columnIndex < targetLength; columnIndex++)
                        {
                            var localCost = lastSubstitutionCost;
                            var deletionCost = previousRowPtr[columnIndex];
                            if (sourceChar != targetPtr[columnIndex])
                            {
                                localCost = Math.Min(lastInsertionCost, localCost);
                                localCost = Math.Min(deletionCost, localCost);
                                localCost++;
                            }
                            lastInsertionCost = localCost;
                            previousRowPtr[columnIndex] = localCost;
                            lastSubstitutionCost = deletionCost;
                        }
                    }
                }

                var result = previousRow[targetSpan.Length - 1];
                ArrayPool<int>.Shared.Return(previousRow);
                return result;
            }
        }
    }

    /// <summary>
    /// Finds the common characters between two given strings.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="target">The target string.</param>
    /// <returns>A ReadOnlySpan containing the common characters between the two inputs.</returns>
    private static ReadOnlySpan<char> CommonChars(this ReadOnlySpan<char> source, ReadOnlySpan<char> target)
    {
        if (source.IsEmpty || target.IsEmpty)
        {
            return ReadOnlySpan<char>.Empty;
        }

        // the distance used for acceptable transpositions
        var halfLength = (Math.Max(source.Length, target.Length) / 2) - 1;

        var common = new Span<char>(new char[source.Length]);
        var count = 0;

        Span<char> lookup = stackalloc char[target.Length];
        target.CopyTo(lookup);

        for (var i = 0; i != source.Length; ++i)
        {
            var ch = source[i];
            // compare char with range of characters to either side
            var searchLength = Math.Min(i + halfLength, target.Length - 1);
            for (var j = Math.Max(0, i - halfLength); j <= searchLength; ++j)
            {
                if (lookup[j] == ch)
                {
                    common[count++] = ch;
                    lookup[j] = '\0';

                    break;
                }
            }
        }

        return common[..count];
    }
}
