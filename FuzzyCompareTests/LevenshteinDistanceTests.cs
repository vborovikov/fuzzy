namespace FuzzyCompareTests;

[TestClass]
public class LevenshteinDistanceTests
{
    [TestMethod]
    public void LevenshteinTest()
    {
        Assert.AreEqual(1, ComparisonMethods.LevenshteinDistance("My string", "My tring"));
        Assert.AreEqual(2, ComparisonMethods.LevenshteinDistance("My string", "M string2"));
        Assert.AreEqual(1, ComparisonMethods.LevenshteinDistance("My string", "My $tring"));
        Assert.AreEqual(3, ComparisonMethods.LevenshteinDistance("kitten", "sitting"));
    }
}
