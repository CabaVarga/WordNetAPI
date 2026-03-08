using System.Collections.Generic;
using System.Linq;
using LAIR.ResourceAPIs.WordNet;

namespace WordNet.Tests;

[TestClass]
public sealed class RelationTraversalTests
{
    private static WordNetEngine? s_engine;

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        string resourcesPath = Test1.FindResourcesDirectoryPublic();
        s_engine = new WordNetEngine(resourcesPath, false);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        s_engine?.Close();
    }

    // ── Direct hypernym/hyponym existence ──────────────────────────────────

    [TestMethod]
    public void GetRelatedSynSets_DogNounMostCommon_HasDirectHypernyms()
    {
        SynSet dog = s_engine!.GetSynSet("Noun:2086723");
        int count = dog.GetRelatedSynSetCount(WordNetEngine.SynSetRelation.Hypernym);

        Assert.IsTrue(count > 0, "Expected at least one hypernym for dog.n.01");
    }

    [TestMethod]
    public void GetRelatedSynSets_DogNounMostCommon_DirectHypernymCount_IsPinned()
    {
        SynSet dog = s_engine!.GetSynSet("Noun:2086723");
        int count = dog.GetRelatedSynSetCount(WordNetEngine.SynSetRelation.Hypernym);

        Assert.AreEqual(2, count, "dog.n.01 should have exactly 2 direct hypernyms in WordNet 3.1");
    }

    [TestMethod]
    public void GetRelatedSynSets_DogNounMostCommon_HasHyponyms()
    {
        SynSet dog = s_engine!.GetSynSet("Noun:2086723");
        int count = dog.GetRelatedSynSetCount(WordNetEngine.SynSetRelation.Hyponym);

        Assert.IsTrue(count > 5, $"Expected many hyponyms (dog breeds) for dog.n.01, got {count}");
    }

    // ── Direct hypernym IDs are pinned ────────────────────────────────────

    [TestMethod]
    public void GetRelatedSynSets_DogNounMostCommon_DirectHypernymIDs_ArePinned()
    {
        SynSet dog = s_engine!.GetSynSet("Noun:2086723");
        List<string> hypernymIDs = dog
            .GetRelatedSynSets(WordNetEngine.SynSetRelation.Hypernym, false)
            .Select(s => s.ID)
            .OrderBy(id => id)
            .ToList();

        // domestic_animal.n.01 (Noun:1320032) and canine.n.02 (Noun:2085998) in WordNet 3.1
        CollectionAssert.AreEqual(
            new[] { "Noun:1320032", "Noun:2085998" },
            hypernymIDs,
            "dog.n.01 direct hypernym IDs changed (domestic_animal.n.01, canine.n.02)");

    }

    // ── Recursive traversal gives strictly more than direct ───────────────

    [TestMethod]
    public void GetRelatedSynSets_DogNounMostCommon_RecursiveHypernymsMoreThanDirect()
    {
        SynSet dog = s_engine!.GetSynSet("Noun:2086723");
        int directCount  = dog.GetRelatedSynSets(WordNetEngine.SynSetRelation.Hypernym, false).Count;
        int recursiveCount = dog.GetRelatedSynSets(WordNetEngine.SynSetRelation.Hypernym, true).Count;

        Assert.IsTrue(recursiveCount > directCount,
            $"Recursive hypernyms ({recursiveCount}) should exceed direct ({directCount})");
    }

    // ── Root entity synset is reachable from dog via hypernym chain ───────

    [TestMethod]
    public void GetRelatedSynSets_DogNounMostCommon_RecursiveHypernyms_ContainsEntityRoot()
    {
        SynSet dog = s_engine!.GetSynSet("Noun:2086723");
        var allHypernyms = dog.GetRelatedSynSets(WordNetEngine.SynSetRelation.Hypernym, true);
        bool containsEntity = allHypernyms.Any(s => s.ID == "Noun:1740");

        Assert.IsTrue(containsEntity, "entity.n.01 (Noun:1740) should be reachable from dog via hypernyms");
    }

    // ── Synset content (words, gloss) ─────────────────────────────────────

    [TestMethod]
    public void GetSynSet_DogMostCommon_WordsContainDog()
    {
        SynSet dog = s_engine!.GetSynSet("Noun:2086723");

        CollectionAssert.Contains(dog.Words, "dog",
            "dog.n.01 words should include \"dog\"");
    }

    [TestMethod]
    public void GetSynSet_DogMostCommon_GlossIsNonEmpty()
    {
        SynSet dog = s_engine!.GetSynSet("Noun:2086723");

        Assert.IsFalse(string.IsNullOrWhiteSpace(dog.Gloss),
            "dog.n.01 should have a non-empty gloss");
    }
}
