namespace FuzzyCompare.Text;

using System;

[Flags]
public enum TokenCategory
{
    None            = 0,
    Other           = 1 << 0,
    LineBreak       = 1 << 1,
    WhiteSpace      = 1 << 2,
    Word            = 1 << 3,
    Number          = 1 << 4,
    OtherNumber     = 1 << 5,
    PunctuationMark = 1 << 6,
    Symbol          = 1 << 7,
}