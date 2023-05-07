namespace FuzzyCompareTests;

[TestClass]
public class JaroWinklerSimilarityTests
{
    [TestMethod]
        public void JaroWinklerSimilarity_Known1()
    {
        var similarity = ComparisonMethods.JaroWinklerSimilarity("DwAyNE", "DuANE");
        Assert.AreEqual(0.840000f, MathF.Round(similarity, 6, MidpointRounding.AwayFromZero));
    }

    [TestMethod]
    public void JaroWinklerSimilarity_Known2()
    {
        var similarity = ComparisonMethods.JaroWinklerSimilarity("TRATE", "TRACE");
        Assert.AreEqual(0.906667f, MathF.Round(similarity, 6, MidpointRounding.AwayFromZero));
    }

    [TestMethod]
    public void JaroWinklerSimilarity_Known3()
    {
        var similarity = ComparisonMethods.JaroWinklerSimilarity("arnab", "aranb");
        Assert.AreEqual(0.946667f, MathF.Round(similarity, 6, MidpointRounding.AwayFromZero));
    }

    [TestMethod]
    public void JaroWinklerSimilarity_Known4()
    {
        var similarity = ComparisonMethods.JaroWinklerSimilarity("My string", "My tsring");
        Assert.AreEqual(0.974074f, MathF.Round(similarity, 6, MidpointRounding.AwayFromZero));
    }

    [TestMethod]
    public void JaroWinklerSimilarity_Known5()
    {
        var similarity = ComparisonMethods.JaroWinklerSimilarity("My string", "My ntrisg");
        Assert.AreEqual(0.896296f, MathF.Round(similarity, 6, MidpointRounding.AwayFromZero));
    }
}