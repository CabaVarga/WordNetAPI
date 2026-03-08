using System;
using System.IO;
using LAIR.ResourceAPIs.WordNet;

namespace WordNet.Tests;

/// <summary>
/// Tests for the explicit index-file preprocessing contract introduced in Phase 2.
/// Normal engine construction must be read-only; sorting must be an explicit step.
/// </summary>
[TestClass]
public sealed class PreprocessingTests
{
    // ── Sorted path: engine loads without mutating any index file ──────────

    [TestMethod]
    public void Constructor_SortedDirectory_DoesNotModifyIndexFiles()
    {
        string resourcesPath = TestHelpers.FindResourcesDirectory();
        string markerPath = Path.Combine(resourcesPath, ".sorted_for_dot_net");

        // Pre-condition: the resources directory must already be sorted.
        Assert.IsTrue(File.Exists(markerPath),
            "Test pre-condition: resources directory must contain .sorted_for_dot_net marker.");

        string[] indexPaths = new string[]
        {
            Path.Combine(resourcesPath, "index.adj"),
            Path.Combine(resourcesPath, "index.adv"),
            Path.Combine(resourcesPath, "index.noun"),
            Path.Combine(resourcesPath, "index.verb"),
        };

        // Capture last-write timestamps before construction.
        DateTime[] before = Array.ConvertAll(indexPaths, p => File.GetLastWriteTimeUtc(p));

        WordNetEngine engine = new WordNetEngine(resourcesPath, false);
        engine.Close();

        // Assert that no index file was written during construction.
        for (int i = 0; i < indexPaths.Length; i++)
        {
            DateTime after = File.GetLastWriteTimeUtc(indexPaths[i]);
            Assert.AreEqual(before[i], after,
                $"Index file was modified during engine construction: {Path.GetFileName(indexPaths[i])}");
        }
    }

    // ── Unsorted path: constructor throws before loading any data ──────────

    [TestMethod]
    public void Constructor_UnsortedDirectory_ThrowsInvalidOperationException()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            // Create the 8 required dummy files — content does not matter because the
            // fail-fast guard fires before any parsing occurs.
            foreach (string name in new[]
            {
                "data.adj", "data.adv", "data.noun", "data.verb",
                "index.adj", "index.adv", "index.noun", "index.verb",
            })
                File.WriteAllText(Path.Combine(tempDir, name), string.Empty);

            // No .sorted_for_dot_net marker — engine must refuse to load.
            Assert.ThrowsException<InvalidOperationException>(
                () => new WordNetEngine(tempDir, true),
                "Expected InvalidOperationException when .sorted_for_dot_net marker is absent.");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}
