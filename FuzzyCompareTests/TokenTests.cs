namespace FuzzyCompareTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class TokenTests
{
    [TestMethod]
    public void Equals_EqualTokensSameSource_AreEqual()
    {
        var tokens = "hello hello".Tokenize().Where(tk => tk.Category == TokenCategory.Word).ToArray();

        Assert.AreEqual(tokens.Length, 2);
        EqualityTests.TestEqualObjects(tokens[0], tokens[1]);
    }

    [TestMethod]
    public void Equals_EqualTokensDiffSources_AreEqual()
    {
        var first = "hello 1 hello".Tokenize().First();
        var second = "hello 2 hello".Tokenize().Last();

        EqualityTests.TestEqualObjects(first, second);
    }

    [TestMethod]
    public void Equals_UnequalTokensSameSource_AreNotEqual()
    {
        var tokens = "hello world".Tokenize().Where(tk => tk.Category == TokenCategory.Word).ToArray();

        Assert.AreEqual(tokens.Length, 2);
        EqualityTests.TestUnequalObjects(tokens[0], tokens[1]);
    }

    [TestMethod]
    public void Equals_UnequalTokensDiffSources_AreNotEqual()
    {
        var first = "hello 1 world".Tokenize().First();
        var second = "hello 2 world".Tokenize().Last();

        EqualityTests.TestUnequalObjects(first, second);
    }

    [TestMethod]
    public void Equals_DefaultToken_Correct()
    {
        var token = "hello world".Tokenize().First();

        EqualityTests.TestAgainstNull(token);
    }

    [TestMethod]
    public void Equals_DefaultToken_AreNotEqual()
    {
        var token = "hello world".Tokenize().First();

        EqualityTests.TestUnequalObjects(token, default);
    }

    [TestMethod]
    public void Tokenize_NumberMinusSign_PartOfNumber()
    {
        var token = "sample -1".Tokenize().Last();

        Assert.AreEqual(TokenCategory.Number, token.Category);
        Assert.AreEqual("-1", token.ToString());
    }

    [TestMethod]
    public void Tokenize_HexNumber_NumberCategory()
    {
        var token = "sample 0xDeadBeef".Tokenize().Last();

        Assert.AreEqual(TokenCategory.Number, token.Category);
        Assert.AreEqual("0xDeadBeef", token.ToString());
    }

    [TestMethod]
    public void Tokenize_MinusHexNumber_MinusOmitted()
    {
        var token = "sample -0xDeadBeef".Tokenize().Last();

        Assert.AreEqual(TokenCategory.Number, token.Category);
        Assert.AreEqual("0xDeadBeef", token.ToString());
    }

    [TestMethod]
    public void Tokenize_MinusOctetNumber_MinusIncluded()
    {
        var token = "sample -015".Tokenize().Last();

        Assert.AreEqual(TokenCategory.Number, token.Category);
        Assert.AreEqual("-015", token.ToString());
    }

    [TestMethod]
    public void Tokenize_LineBreak_Recognized()
    {
        var token = "sample\r\ntext".Tokenize().SingleOrDefault(tk => tk.Category == TokenCategory.LineBreak);

        Assert.AreEqual(TokenCategory.LineBreak, token.Category);
    }
}
