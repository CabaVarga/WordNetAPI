using LAIR.ResourceAPIs.WordNet;

namespace WordNet.Tests;

[TestClass]
public sealed class SimilarityModelTests
{
    private static WordNetEngine? s_engine;
    private static WordNetSimilarityModel? s_model;

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        string resourcesPath = TestHelpers.FindResourcesDirectory();
        s_engine = new WordNetEngine(resourcesPath, false);
        s_model  = new WordNetSimilarityModel(s_engine);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        s_engine?.Close();
    }

    // ── Self-similarity ────────────────────────────────────────────────────

    [TestMethod]
    public void GetSimilarity_DogWithItself_WuPalmerMostCommon_ReturnsOne()
    {
        float sim = s_model!.GetSimilarity(
            "dog",  WordNetEngine.POS.Noun,
            "dog",  WordNetEngine.POS.Noun,
            WordNetSimilarityModel.Strategy.WuPalmer1994MostCommon,
            WordNetEngine.SynSetRelation.Hypernym);

        Assert.AreEqual(1.0f, sim, 0.0001f, "A concept should be identical to itself");
    }

    // ── Same-POS word pairs ────────────────────────────────────────────────

    [TestMethod]
    public void GetSimilarity_DogAndCatNoun_WuPalmerMostCommon_IsBetweenZeroAndOne()
    {
        float sim = s_model!.GetSimilarity(
            "dog", WordNetEngine.POS.Noun,
            "cat", WordNetEngine.POS.Noun,
            WordNetSimilarityModel.Strategy.WuPalmer1994MostCommon,
            WordNetEngine.SynSetRelation.Hypernym);

        Assert.IsTrue(sim > 0f && sim < 1f,
            $"dog–cat similarity should be in (0, 1); got {sim}");
    }

    [TestMethod]
    public void GetSimilarity_DogAndCatNoun_GreaterThan_DogAndCarNoun_WuPalmerMax()
    {
        float dogCat = s_model!.GetSimilarity(
            "dog", WordNetEngine.POS.Noun,
            "cat", WordNetEngine.POS.Noun,
            WordNetSimilarityModel.Strategy.WuPalmer1994Maximum,
            WordNetEngine.SynSetRelation.Hypernym);

        float dogCar = s_model.GetSimilarity(
            "dog", WordNetEngine.POS.Noun,
            "car", WordNetEngine.POS.Noun,
            WordNetSimilarityModel.Strategy.WuPalmer1994Maximum,
            WordNetEngine.SynSetRelation.Hypernym);

        Assert.IsTrue(dogCat > dogCar,
            $"dog–cat ({dogCat:F4}) should be more similar than dog–car ({dogCar:F4})");
    }

    // ── Cross-POS: no shared hypernym chain ───────────────────────────────

    [TestMethod]
    public void GetSimilarity_DogNounAndRunVerb_WuPalmerMostCommon_ReturnsZero()
    {
        float sim = s_model!.GetSimilarity(
            "dog", WordNetEngine.POS.Noun,
            "run", WordNetEngine.POS.Verb,
            WordNetSimilarityModel.Strategy.WuPalmer1994MostCommon,
            WordNetEngine.SynSetRelation.Hypernym);

        Assert.AreEqual(0f, sim, 0.0001f,
            "Cross-POS noun–verb similarity via hypernym only should be 0 (no shared ancestor)");
    }

    // ── Average strategy returns valid range ──────────────────────────────

    [TestMethod]
    public void GetSimilarity_DogAndCatNoun_WuPalmerAverage_IsBetweenZeroAndOne()
    {
        float sim = s_model!.GetSimilarity(
            "dog", WordNetEngine.POS.Noun,
            "cat", WordNetEngine.POS.Noun,
            WordNetSimilarityModel.Strategy.WuPalmer1994Average,
            WordNetEngine.SynSetRelation.Hypernym);

        Assert.IsTrue(sim >= 0f && sim <= 1f,
            $"Average WuPalmer similarity should be in [0, 1]; got {sim}");
    }
}
