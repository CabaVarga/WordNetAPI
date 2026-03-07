using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LAIR.ResourceAPIs.WordNet;

namespace WordNet.Tests;

[TestClass]
public sealed class Test1
{
    private static WordNetEngine? s_engine;

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        string resourcesPath = FindResourcesDirectory();
        s_engine = new WordNetEngine(resourcesPath, false);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        if (s_engine != null)
            s_engine.Close();
    }

    [TestMethod]
    public void GetSynSets_DogNoun_ReturnsExpectedIDs()
    {
        List<string> ids = s_engine!
            .GetSynSets("dog", WordNetEngine.POS.Noun)
            .Select(s => s.ID)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();

        CollectionAssert.AreEqual(
            new[]
            {
                "Noun:10042764",
                "Noun:10133978",
                "Noun:2086723",
                "Noun:2712903",
                "Noun:3907626",
                "Noun:7692347",
                "Noun:9905672"
            },
            ids);
    }

    [TestMethod]
    public void GetMostCommonSynSet_DogNoun_ReturnsExpectedID()
    {
        SynSet? mostCommon = s_engine!.GetMostCommonSynSet("dog", WordNetEngine.POS.Noun);

        Assert.IsNotNull(mostCommon);
        Assert.AreEqual("Noun:2086723", mostCommon.ID);
    }

    [TestMethod]
    public void GetMostCommonSynSet_RunVerb_ReturnsExpectedID()
    {
        SynSet? mostCommon = s_engine!.GetMostCommonSynSet("run", WordNetEngine.POS.Verb);

        Assert.IsNotNull(mostCommon);
        Assert.AreEqual("Verb:1930264", mostCommon.ID);
    }

    [TestMethod]
    public void GetSynSets_NormalizesCaseAndSpaces()
    {
        int normalizedCount = s_engine!.GetSynSets("new york", WordNetEngine.POS.Noun).Count;
        int mixedCaseCount = s_engine.GetSynSets("New York", WordNetEngine.POS.Noun).Count;

        Assert.AreEqual(3, normalizedCount);
        Assert.AreEqual(normalizedCount, mixedCaseCount);
    }

    [TestMethod]
    public void GetSynSets_UnknownWord_ReturnsEmptySet()
    {
        int count = s_engine!.GetSynSets("zzzznotaword", WordNetEngine.POS.Noun).Count;

        Assert.AreEqual(0, count);
    }

    private static string FindResourcesDirectory()
    {
        DirectoryInfo? current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            string candidate = Path.Combine(current.FullName, "resources");
            if (Directory.Exists(candidate))
                return candidate;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository resources directory.");
    }
}
