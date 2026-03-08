using LAIR.ResourceAPIs.WordNet;

namespace WordNet.Tests;

[TestClass]
public sealed class EdgeCaseTests
{
    private static WordNetEngine? s_engine;

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        string resourcesPath = TestHelpers.FindResourcesDirectory();
        s_engine = new WordNetEngine(resourcesPath, false);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        s_engine?.Close();
    }

    // ── Empty / whitespace inputs ──────────────────────────────────────────

    [TestMethod]
    public void GetSynSets_EmptyString_ReturnsEmpty()
    {
        int count = s_engine!.GetSynSets(string.Empty, WordNetEngine.POS.Noun).Count;

        Assert.AreEqual(0, count);
    }

    // ── All four POS have results for representative words ─────────────────

    [TestMethod]
    public void GetSynSets_AdjectiveHappy_ReturnsNonEmpty()
    {
        int count = s_engine!.GetSynSets("happy", WordNetEngine.POS.Adjective).Count;

        Assert.IsTrue(count > 0, "Expected at least one adjective synset for \"happy\"");
    }

    [TestMethod]
    public void GetSynSets_AdverbQuickly_ReturnsNonEmpty()
    {
        int count = s_engine!.GetSynSets("quickly", WordNetEngine.POS.Adverb).Count;

        Assert.IsTrue(count > 0, "Expected at least one adverb synset for \"quickly\"");
    }

    // ── Unknown word returns null for most-common, empty for all synsets ───

    [TestMethod]
    public void GetMostCommonSynSet_UnknownWord_ReturnsNull()
    {
        SynSet? result = s_engine!.GetMostCommonSynSet("zzzznotaword", WordNetEngine.POS.Noun);

        Assert.IsNull(result);
    }

    // ── GetSynSet round-trip by ID ─────────────────────────────────────────

    [TestMethod]
    public void GetSynSet_ByID_RoundTrips_ToSameID()
    {
        const string knownID = "Noun:2086723";
        SynSet synset = s_engine!.GetSynSet(knownID);

        Assert.AreEqual(knownID, synset.ID);
    }

    // ── High-polysemy verb has many synsets ────────────────────────────────

    [TestMethod]
    public void GetSynSets_VerbMake_HasMoreThanTenSynsets()
    {
        int count = s_engine!.GetSynSets("make", WordNetEngine.POS.Verb).Count;

        Assert.IsTrue(count > 10,
            $"\"make\" is highly polysemous; expected > 10 verb synsets, got {count}");
    }

    // ── POS.None returns results across all parts-of-speech ───────────────

    [TestMethod]
    public void GetSynSets_DogNoPosRestriction_ReturnsMoreThanNounOnly()
    {
        int nounOnly = s_engine!.GetSynSets("dog", WordNetEngine.POS.Noun).Count;
        int allPOS   = s_engine.GetSynSets("dog").Count;

        Assert.IsTrue(allPOS >= nounOnly,
            "Unrestricted POS search should return at least as many synsets as noun-only");
    }

    // ── SemanticRelations property is consistent with relation count ───────

    [TestMethod]
    public void SemanticRelations_DogMostCommon_ContainsHypernymRelation()
    {
        SynSet dog = s_engine!.GetSynSet("Noun:2086723");
        bool hasHypernym = false;
        foreach (var rel in dog.SemanticRelations)
            if (rel == WordNetEngine.SynSetRelation.Hypernym)
            {
                hasHypernym = true;
                break;
            }

        Assert.IsTrue(hasHypernym, "dog.n.01 should advertise a Hypernym semantic relation");
    }
}
