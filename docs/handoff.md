# WordNetAPI Handoff

Date: 2026-03-08 (updated 2026-03-08, session 4)

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
- [x] `docs/handoff-archive.md` updated with Session 4 entry (first commit on branch).

### Pending

- [ ] A1: Remove `LAIR.Extensions` from `WordNetEngine.cs` and `SynSet.cs` (7 call sites for `EnsureContainsKey`, multiple `TryReadLine` loops, 1 `SetPosition`).
- [ ] A2: Replace `BinarySearchTextStream` with internal helper.
- [ ] A3.1: Add internal `Set<T>` shim; swap all usages.
- [ ] Remove `LAIR.*` from `WordNet.csproj`; confirm clean build and 28/28 tests.
- [ ] Remove `LAIR.*` from `TestApplication.csproj` and `WordNet.Tests.csproj`.
- [ ] Add dependency provenance note to docs.

## Recommended immediate next steps

1. **Start with A1** — replace the three `LAIR.Extensions` patterns in `WordNetEngine.cs` and `SynSet.cs`. No behavior change; purely mechanical substitutions. Run tests after each file.
2. **A2** — implement `IndexBinarySearchReader` (internal class) in `src/WordNet/`; replace the `BinarySearchTextStream` usage in the disk-mode init block.
3. **A3.1** — add `src/WordNet/Internal/Set.cs` shim; swap usages across `SynSet.cs`, `WordNetEngine.cs`.

See `docs/lair-dependencies.md` for the full usage map and replacement table.

## Quick restart prompt (for new chat)

```text
Use D:\WordNetAPI-fork on branch feature/phase-3.
Read docs/handoff.md, docs/modernization-plan.md, and docs/lair-dependencies.md.
Phases 0–2 are merged to master. Phase 3 is active — working tree is clean.
28/28 tests pass. Do not push until instructed.
Phase 3 goal: remove LAIR.* dependencies from WordNet core using Option A (inline replacements).
Start with A1: remove LAIR.Extensions usage from WordNetEngine.cs and SynSet.cs.

WORKFLOW REMINDER: When opening a new branch, add a new entry to docs/handoff-archive.md
as the first or second commit, before any implementation work. Record the trigger, references
(PR, CI run, test count), changes made to handoff.md, and any complications from the prior session.
```
