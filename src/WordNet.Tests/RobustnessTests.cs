using System;
using System.Threading;
using System.Threading.Tasks;
using LAIR.ResourceAPIs.WordNet;

namespace WordNet.Tests;

[TestClass]
public sealed class RobustnessTests
{
    [TestMethod]
    public void Dispose_CanBeCalledTwice()
    {
        string resourcesPath = TestHelpers.FindResourcesDirectory();
        WordNetEngine engine = new WordNetEngine(resourcesPath, false);

        engine.Dispose();
        engine.Dispose();
    }

    [TestMethod]
    public void CloseThenUse_ThrowsObjectDisposedException()
    {
        string resourcesPath = TestHelpers.FindResourcesDirectory();
        WordNetEngine engine = new WordNetEngine(resourcesPath, false);
        engine.Close();

        Assert.ThrowsException<ObjectDisposedException>(
            () => engine.GetSynSets("dog", WordNetEngine.POS.Noun));
    }

    [TestMethod]
    public void GetSynSets_PosNoneRestriction_ThrowsArgumentException()
    {
        string resourcesPath = TestHelpers.FindResourcesDirectory();
        using WordNetEngine engine = new WordNetEngine(resourcesPath, false);

        ArgumentException ex = Assert.ThrowsException<ArgumentException>(
            () => engine.GetSynSets("dog", WordNetEngine.POS.None));

        Assert.AreEqual("posRestriction", ex.ParamName);
    }

    [TestMethod]
    public void GetSynSets_DiskMode_ConcurrentReads_DoNotFail()
    {
        string resourcesPath = TestHelpers.FindResourcesDirectory();
        using WordNetEngine engine = new WordNetEngine(resourcesPath, false);

        int failures = 0;
        Parallel.For(0, 24, _ =>
        {
            int count = engine.GetSynSets("dog", WordNetEngine.POS.Noun).Count;
            if (count == 0)
                Interlocked.Increment(ref failures);
        });

        Assert.AreEqual(0, failures, "Concurrent disk reads returned unexpected empty results.");
    }
}
