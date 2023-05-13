# FuzzyCompare
Fuzzy string comparison library

[![Downloads](https://img.shields.io/nuget/dt/FuzzyCompare.svg)](https://www.nuget.org/packages/FuzzyCompare)
[![NuGet](https://img.shields.io/nuget/v/FuzzyCompare.svg)](https://www.nuget.org/packages/FuzzyCompare)
[![MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/vborovikov/fuzzy/blob/main/LICENSE)

## ComparisonMethods

The class provides three static methods:

- `JaroSimilarity`: calculates the Jaro similarity between two given `ReadOnlySpan<char>` inputs. It returns a float value that ranges from 0 to 1, representing the similarity between the two inputs.
- `JaroWinklerSimilarity`: calculates the Jaro-Winkler similarity between two given `ReadOnlySpan<char>` inputs. It returns a float value that ranges from 0 to 1, representing the similarity between the two inputs. The p parameter is optional and it sets the scaling factor for the common prefix length adjustment.
- `LevenshteinDistance`: calculates the Levenshtein distance between two given `ReadOnlySpan<char>` inputs. It returns an int value that represents the minimum number of single-character edits (insertions, deletions, or substitutions) required to change one input into the other.

## FuzzyStringComparer

The `FuzzyStringComparer` class provides a way to compare strings using a fuzzy matching algorithm. The main purpose of the `FuzzyStringComparer` class is to determine the similarity between two strings, by calculating a similarity score based on the number of matching characters and their positions within the strings.

It provides a constructor that takes an optional CultureInfo parameter, which can be used to specify the culture to use for string comparison. By default, the `FuzzyStringComparer` class uses the current culture for case-insensitive string comparison.

The `FuzzyStringComparer` class uses a fuzzy matching algorithm that takes into account several factors, including the length of the strings, the number of matching characters, and the positions of the matching characters within the strings. The algorithm is designed to be tolerant of small differences between the strings, such as typos, misspellings, or minor variations in formatting.

## Tokenizer

The `Tokenizer` class is a utility class that provides methods for tokenizing strings and spans of characters. The main purpose of the class is to break down a string or span of characters into individual tokens, which are then returned as a token enumerator.

The class provides two main methods for tokenizing strings and spans of characters: `Tokenize` and `EnumerateTokens`. The `Tokenize` method takes a string as input and returns an `TokenEnumerator` object that can be used to iterate through the tokens in the string. The `EnumerateTokens` method takes a read-only span of characters as input and returns a `TokenRefEnumerator` object that can be used to iterate through the tokens in the span.