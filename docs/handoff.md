# WordNetAPI Handoff

Date: 2026-03-08 (updated 2026-03-08, session 7)

## Current working context

- Local repo: `D:\WordNetAPI-fork`
- Active branch: `feature/phase-3`
- Fork repo: `https://github.com/CabaVarga/WordNetAPI.git`
- Upstream repo: `https://github.com/zacg/WordNetAPI.git`
- `gh` CLI is installed/authenticated and default repo is set to `CabaVarga/WordNetAPI`.

## Phase history

### Phase 0 — complete (merged to `master` via PR #1)

- CI workflow (`.github/workflows/ci-build.yml`): restore + build matrix on `windows-2022` (required) / `windows-2025` (canary).
- `Directory.Build.props`: `FrameworkPathOverride` workaround for net40 on hosted runners.
- `global.json`: SDK pinned to `9.0.300`.
- `src/WordNet.Tests`: SDK-style test project (`net48`) scaffolded with MSTest; initial fixture and 5 smoke tests in `Test1.cs`.
- `AllRules.ruleset` warnings removed from both legacy project files.
- `README.md` expanded with prerequisites, build commands, data layout, project table, CI badge.
- Full modernization docs suite added (`docs/`).
- CI green on both runners: run `22820942946`.

### Phase 1 — complete (merged to `master` via PR #2)

- 26 passing tests across 4 files: `Test1.cs`, `RelationTraversalTests.cs`, `SimilarityModelTests.cs`, `EdgeCaseTests.cs`.
- `dotnet test` step added to `.github/workflows/ci-build.yml`.
- Shared `TestHelpers.FindResourcesDirectory()` helper factored out.
- CI green on both runners.

### Phase 2 — complete (merged to `master` via PR #3)

- `WordNetEngine.SortIndexFiles(wordNetDirectory)` extracted as explicit preprocessing step.
- Constructor now throws `InvalidOperationException` if `.sorted_for_dot_net` marker is absent.
- `PreprocessingTests.cs` added: 2 tests covering sorted and unsorted paths.
- `.gitattributes` added: `resources/data.*` and `resources/index.*` marked as binary to prevent CRLF corruption of byte offsets on Windows CI.
- 28/28 tests pass in CI on both runners.

## Current status (Phase 3 — active on `feature/phase-3`)

### Goal

Stabilize the `LAIR.*` dependency story so the library builds reproducibly from a clean checkout without machine-specific state. The chosen strategy (Option A from `docs/lair-dependencies.md`) is to **inline/replace the minimal LAIR functionality** in three staged steps, then remove the DLL references.

### Staged plan (from `docs/lair-dependencies.md` Option A)

| Step | What | Risk |
|---|---|---|
| A1 | Remove `LAIR.Extensions` usage: `EnsureContainsKey` → explicit init, `TryReadLine` → `ReadLine()` null-check, `SetPosition` → `DiscardBufferedData + BaseStream.Position` | Low |
| A2 | Replace `LAIR.IO.BinarySearchTextStream` with an internal `IndexBinarySearchReader` helper | Medium |
| A3.1 | Introduce internal `Set<T>` shim preserving `AddRange`, `IsReadOnly`, used constructors | Medium-high |
| — | Remove `LAIR.*` refs from `WordNet.csproj`; validate with characterization tests | — |
| — | Remove LAIR refs from `TestApplication.csproj` and `WordNet.Tests.csproj` | — |

### Done

- [x] Branch `feature/phase-3` created from updated `master` (post PR #3 merge).
- [x] 28/28 tests confirmed passing on `master`.
- [x] `docs/handoff-archive.md` updated with Session 4 and 5 entries.
- [x] **A1: `LAIR.Extensions` removed from `WordNetEngine.cs` and `SynSet.cs`.**
  - `using LAIR.Extensions;` removed from both files.
  - 7 `EnsureContainsKey` calls (6 in `SynSet.cs`, 1 in `WordNetEngine.cs`) → explicit `ContainsKey` + `new`.
  - 6 `TryReadLine` loops (in `SortIndexFiles` and constructor) → `ReadLine()` null-check + explicit `Close()`.
  - 1 `SetPosition(0)` → `DiscardBufferedData(); BaseStream.Position = 0`.
  - 28/28 tests pass; clean build (0 warnings, 0 errors).
- [x] **A2: `LAIR.IO.BinarySearchTextStream` replaced with internal implementation.**
  - `src/WordNet/Internal/BinarySearchTextStream.cs` added — `internal` class in `LAIR.ResourceAPIs.WordNet` namespace matching the used API surface: `SearchComparisonDelegate`, constructor `(string path, ...)`, `Search(object key)`, `Stream` property, `Close()`.
  - Binary search operates directly on `FileStream` at byte level (avoids `StreamReader` buffering issues); `StreamReader` exposed via `Stream` for linear reads in `AllWords`.
  - `using LAIR.IO;` removed from `WordNetEngine.cs`. No other code changes needed — type name resolves to the internal class.
  - `<Compile Include>` added to legacy `WordNet.csproj`.
  - 28/28 tests pass; clean build (0 warnings, 0 errors).
- [x] **A3.1: Internal `Set<T>` shim added; all LAIR references removed.**
  - `src/WordNet/Internal/Set.cs` added — `public class Set<T>` in `LAIR.Collections.Generic` namespace backed by `HashSet<T>`. Preserves used API surface: constructors `()`, `(int)`, `(ICollection<T>)`, `(bool)`; members `Add`, `AddRange`, `Contains`, `Count`, `IsReadOnly` (get/set); `IEnumerable<T>`.
  - `IsReadOnly` setter enforced: protects cached in-memory synset collections from caller mutation (used in `GetSynSets` optimization path).
  - `using LAIR.Collections.Generic;` statements in `SynSet.cs`, `WordNetEngine.cs`, `WordNetSimilarityModel.cs` now resolve to the internal shim — no code changes needed in those files.
  - All three `LAIR.*` DLL references removed from `WordNet.csproj`, `TestApplication.csproj`, and `WordNet.Tests.csproj`.
  - 28/28 tests pass; clean build (0 warnings, 0 errors) across all three projects.

### Pending

- [ ] PR #4 for Phase 3 — ready to open.

## Recommended immediate next steps

1. Merge PR #4 for Phase 3.
2. Begin Phase 4 on a new branch (API Robustness — `IDisposable`, defensive validation, typed exceptions).

See `docs/lair-dependencies.md` for the full usage map and replacement table.

## Quick restart prompt (for new chat)

```text
Use D:\WordNetAPI-fork on branch master (after merging PR #4).
Read docs/handoff.md, docs/modernization-plan.md.
Phases 0–3 are merged to master. 28/28 tests pass. All LAIR.* dependencies removed.
Begin Phase 4: API Robustness (IDisposable, defensive validation, typed exceptions).

WORKFLOW REMINDER: When opening a new branch, add a new entry to docs/handoff-archive.md
as the first or second commit, before any implementation work. Record the trigger, references
(PR, CI run, test count), changes made to handoff.md, and any complications from the prior session.
```
