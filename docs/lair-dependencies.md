# LAIR Dependency Analysis and Removal

Date: 2026-03-07 (updated 2026-03-08)

## Goal

Identify all dependencies on `LAIR.*` DLLs in this repository and propose practical ways to remove them with controlled risk.

---

## Dependency Inventory

## Referenced LAIR assemblies

The codebase currently references:

- `LAIR.Collections.dll`
- `LAIR.Extensions.dll`
- `LAIR.IO.dll`

Project-level references exist in:

- `src/WordNet/WordNet.csproj`
- `src/TestApplication/TestApplication.csproj`
- `src/WordNet.Tests/WordNet.Tests.csproj` (added during modernization work)

## Where each DLL is used

### 1) `LAIR.Collections` (high usage)

Primary type in use: `LAIR.Collections.Generic.Set<T>`

Usage footprint:

- `src/WordNet/SynSet.cs` (`Set<...>` appears 27 times)
- `src/WordNet/WordNetEngine.cs` (`Set<...>` appears 22 times)
- `src/TestApplication/TestForm.cs` (`Set<...>` appears 6 times)
- `src/WordNet/WordNetSimilarityModel.cs` (namespace import only, no direct `Set<T>` use)

Notable API reliance:

- constructors like `new Set<T>()`, `new Set<T>(capacity)`, `new Set<T>(ICollection<T>)`
- `AddRange(...)`
- `IsReadOnly` setter in `WordNetEngine.GetSynSets(...)`

### 2) `LAIR.Extensions` (medium usage)

Used for extension methods:

- `DictionaryExtensions.EnsureContainsKey(...)`
  - `SynSet.cs` (6 call sites)
  - `WordNetEngine.cs` (1 call site)
- `StreamReaderExtensions.TryReadLine(...)`
  - `WordNetEngine.cs` (multiple file-scan loops)
- `StreamReaderExtensions.SetPosition(...)`
  - `WordNetEngine.cs` (`AllWords` disk-mode branch)

### 3) `LAIR.IO` (narrow but important usage)

Used type:

- `LAIR.IO.BinarySearchTextStream`
  - `WordNetEngine.cs` only

It is central to disk-mode word lookup via binary search on `index.*`.

---

## Functional Dependency Map (What really matters)

From a behavior perspective, the WordNet engine depends on LAIR for only three capabilities:

1. **Set container** with a few convenience features (`AddRange`, `IsReadOnly` flag).
2. **Dictionary/stream helper extensions** (`EnsureContainsKey`, `TryReadLine`, `SetPosition`).
3. **Binary search over text files** (`BinarySearchTextStream`).

Everything else in LAIR appears unused by this repository.

---

## Removal Strategy Options

## Option A (recommended): Internal compatibility layer, then swap internals

This is lowest-risk for legacy behavior.

### Phase A1 - Remove `LAIR.Extensions` first (easy)

Replace:

- `EnsureContainsKey` with explicit `TryGetValue`/`ContainsKey` + assignment
- `TryReadLine(out line)` with `line = reader.ReadLine(); line != null`
- `SetPosition(0)` with `DiscardBufferedData(); BaseStream.Position = 0`

Impact:

- No public API change
- Very low risk

### Phase A2 - Replace `LAIR.IO.BinarySearchTextStream` (moderate)

Implement local index lookup in `WordNetEngine`:

- either a new internal helper class (`IndexBinarySearchReader`)
- or preload `index.*` word->line dictionary for disk mode only

Trade-offs:

- True binary-search replacement preserves memory profile better.
- Dictionary preload is simpler but increases memory (still likely acceptable relative to full in-memory mode).

### Phase A3 - Replace `LAIR.Collections.Set<T>` (highest)

Two sub-options:

- **A3.1 compatibility wrapper (safer):**
  - Introduce internal `Set<T>` shim in the project that mimics used members (`AddRange`, `IsReadOnly`, constructors).
  - Keep public signatures unchanged initially.
- **A3.2 direct migration to `HashSet<T>` (cleaner but breaking):**
  - Replace public return/parameter types from `Set<T>` to BCL collections (`IReadOnlyCollection<T>`, `HashSet<T>`, or `IEnumerable<T>`).
  - Requires API migration for consumers.

Recommendation:

- Do A3.1 first, then later consider A3.2 as a versioned API cleanup.

## Option B: Keep LAIR but vendor source/binaries

Pros:

- Fastest short-term stabilization

Cons:

- Does not actually remove dependency
- Continues carrying legacy external API surface

Use only as temporary mitigation.

## Option C: Full API modernization now

- Replace all `Set<T>` public surface immediately with BCL types.
- Remove all LAIR references in one major change.

Pros:

- Clean end-state quickly

Cons:

- Higher regression risk and consumer breakage
- Not ideal before broader characterization tests are in place

---

## Proposed Execution Plan (Recommended)

1. **Remove `LAIR.Extensions` usage** in `WordNet` code.
2. **Replace `BinarySearchTextStream`** with internal helper in `WordNet`.
3. **Introduce local `Set<T>` compatibility shim** inside `WordNet` project.
4. Remove LAIR references from `WordNet.csproj`.
5. Validate with characterization tests (`WordNet.Tests`).
6. Remove LAIR refs from `TestApplication.csproj` and `WordNet.Tests.csproj`.
7. Optional later: migrate public API from custom `Set<T>` to BCL types in a breaking-change release.

---

## Concrete Replacement Mapping

| Current dependency | Current usage | Proposed replacement |
|---|---|---|
| `LAIR.Collections.Set<T>` | container + `AddRange` + `IsReadOnly` | internal `Set<T>` shim or `HashSet<T>` + helper methods |
| `EnsureContainsKey` | dictionary auto-init | explicit `TryGetValue` + assignment |
| `TryReadLine` | stream loops | plain `ReadLine()` null-check |
| `SetPosition` | reset reader to start | `DiscardBufferedData()` + `BaseStream.Position = ...` |
| `BinarySearchTextStream` | on-disk index lookups | internal binary-search helper or in-memory word->line index |

---

## Risk Assessment

- **Low risk:** removing extension methods (`LAIR.Extensions`)
- **Medium risk:** replacing binary search implementation (`LAIR.IO`)
- **Medium-high risk:** replacing `Set<T>` while preserving behavior (`IsReadOnly`, enumeration semantics)

Main regression vectors:

- Disk-mode lookup correctness/performance
- Most-common-sense selection behavior (index offset ordering assumptions)
- Public API compatibility for external consumers relying on `Set<T>`

---

## Acceptance Criteria for "LAIR-free" WordNet Core

- `src/WordNet/WordNet.csproj` has no `LAIR.*` references.
- `WordNet` builds and tests pass from clean checkout.
- Existing characterization tests continue passing.
- Disk-mode and in-memory mode parity maintained for tested scenarios.

---

## Notes specific to this repository

- The namespace `LAIR.ResourceAPIs.WordNet` can remain unchanged independently of DLL removal.
- Current tests already exposed a thread-safety constraint in disk mode; keep test execution non-parallel while refactoring internals.
- Removing LAIR from core first is enough to decouple future modernization; sample app and tests can follow immediately after.

---

## Dependency Provenance (post-removal record)

**Status: All `LAIR.*` DLL references removed.** Completed 2026-03-08 on `feature/phase-3`.

The three external DLLs (`LAIR.Collections.dll`, `LAIR.Extensions.dll`, `LAIR.IO.dll`) in `lib/` are
no longer referenced by any project. They remain in the repository for historical reference only and
can be deleted in a future cleanup commit.

### What replaced them

| Original LAIR type / method | Replacement | Location |
|---|---|---|
| `LAIR.Collections.Generic.Set<T>` | Internal `Set<T>` backed by `HashSet<T>` | `src/WordNet/Internal/Set.cs` |
| `LAIR.IO.BinarySearchTextStream` | Internal byte-level binary search over `FileStream` | `src/WordNet/Internal/BinarySearchTextStream.cs` |
| `LAIR.Extensions.DictionaryExtensions.EnsureContainsKey` | Explicit `ContainsKey` + assignment | Inlined at each call site |
| `LAIR.Extensions.StreamReaderExtensions.TryReadLine` | `ReadLine()` null-check pattern | Inlined at each call site |
| `LAIR.Extensions.StreamReaderExtensions.SetPosition` | `DiscardBufferedData(); BaseStream.Position = 0` | Inlined at each call site |

### Design decisions

- **`Set<T>` stays in namespace `LAIR.Collections.Generic`** so that existing `using` directives
  and public API signatures (`GetSynSets`, `GetRelatedSynSets`, `AllWords`, `GetLexicallyRelatedWords`)
  remain source-compatible. A future Phase (A3.2) may migrate the public surface to BCL types
  (`HashSet<T>`, `IReadOnlyCollection<T>`) as a versioned breaking change.
- **`BinarySearchTextStream` is `internal`** in the `LAIR.ResourceAPIs.WordNet` namespace (not the
  LAIR namespace) because it was never part of the public API.
- **`Set<T>` is `public`** because it appears in public method return types and parameters.
- The `Set<T>` capacity constructor accepts an `int` for API compatibility but does not forward it
  to `HashSet<T>` (the `HashSet<T>(int)` constructor is unavailable on the net40 target).
- The `Set<T>(bool throwExceptionOnDuplicateAdd)` constructor is preserved for compatibility with
  `SynSet.GetLexicallyRelatedWords` but the flag is not enforced; `HashSet<T>` silently ignores
  duplicate adds, which matches runtime behavior (no existing code path triggers a duplicate-add
  exception).

### Validation

- 28/28 characterization tests pass (in-memory and disk-mode paths).
- Clean build: 0 warnings, 0 errors across `WordNet`, `TestApplication`, and `WordNet.Tests`.
- No source changes required in `SynSet.cs`, `WordNetEngine.cs`, or `WordNetSimilarityModel.cs` —
  the `using LAIR.Collections.Generic;` statements resolve to the shim type exported from the
  WordNet assembly.
