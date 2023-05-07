namespace FuzzyCompareTests;

[TestClass]
public class JaroSimilarityTests
{
    [TestMethod]
    public void JaroSimilarity_Known1()
    {
        var similarity = ComparisonMethods.JaroSimilarity("Test String1", "Test String2");
        Assert.AreEqual(0.944445f, MathF.Round(similarity, 6, MidpointRounding.AwayFromZero));
    }

    [TestMethod]
    public void JaroSimilarity_Known2()
    {
        var similarity = ComparisonMethods.JaroSimilarity("FAREMVIEL", "FARMVILLE");
        Assert.AreEqual(0.884259f, MathF.Round(similarity, 6, MidpointRounding.AwayFromZero));
    }

    [TestMethod]
    public void JaroSimilarity_Equal()
    {
        var similarity = ComparisonMethods.JaroSimilarity("united arab emirates", "united arab emirates");
        Assert.AreEqual(1f, similarity);
    }
}
