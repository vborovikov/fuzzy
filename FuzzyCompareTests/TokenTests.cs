namespace FuzzyCompareTests;

[TestClass]
public class TokenTests
{
    [TestMethod]
    public void Equals_EqualTokensSameSource_AreEqual()
    {
        var tokens = "hello hello".EnumerateTokens().Where(tk => tk.Category == TokenCategory.Word).ToArray();

        Assert.AreEqual(tokens.Length, 2);
        EqualityTests.TestEqualObjects(tokens[0], tokens[1]);
    }

    [TestMethod]
    public void Equals_EqualTokensDiffSources_AreEqual()
    {
        var first = "hello 1 hello".EnumerateTokens().First();
        var second = "hello 2 hello".EnumerateTokens().Last();

        EqualityTests.TestEqualObjects(first, second);
    }

    [TestMethod]
    public void Equals_UnequalTokensSameSource_AreNotEqual()
    {
        var tokens = "hello world".EnumerateTokens().Where(tk => tk.Category == TokenCategory.Word).ToArray();

        Assert.AreEqual(tokens.Length, 2);
        EqualityTests.TestUnequalObjects(tokens[0], tokens[1]);
    }

    [TestMethod]
    public void Equals_UnequalTokensDiffSources_AreNotEqual()
    {
        var first = "hello 1 world".EnumerateTokens().First();
        var second = "hello 2 world".EnumerateTokens().Last();

        EqualityTests.TestUnequalObjects(first, second);
    }

    [TestMethod]
    public void Equals_DefaultToken_Correct()
    {
        var token = "hello world".EnumerateTokens().First();

        EqualityTests.TestAgainstNull(token);
    }

    [TestMethod]
    public void Equals_DefaultToken_AreNotEqual()
    {
        var token = "hello world".EnumerateTokens().First();

        EqualityTests.TestUnequalObjects(token, default);
    }
}
