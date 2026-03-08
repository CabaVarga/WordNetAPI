# WordNetAPI Handoff

Date: 2026-03-08 (updated 2026-03-08, session 12)

## Current working context

- Local repo: `D:\WordNetAPI-fork`
- Active branch: `feature/phase-5`
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

### Phase 3 — complete (merged to `master` via PR #4)

- All `LAIR.*` DLL dependencies removed via Option A (inline replacements).
- A1: `LAIR.Extensions` usage inlined (7 `EnsureContainsKey`, 6 `TryReadLine`, 1 `SetPosition`).
- A2: `LAIR.IO.BinarySearchTextStream` replaced with internal byte-level implementation (`src/WordNet/Internal/BinarySearchTextStream.cs`).
- A3.1: Internal `Set<T>` shim added (`src/WordNet/Internal/Set.cs`) — `public class Set<T>` in `LAIR.Collections.Generic` namespace backed by `HashSet<T>`.
- All `LAIR.*` references removed from `WordNet.csproj`, `TestApplication.csproj`, `WordNet.Tests.csproj`.
- Dependency provenance documented in `docs/lair-dependencies.md`.
- 28/28 tests pass; 0 warnings, 0 errors across all projects.
- CI green on both runners: runs `22825302479` / `22825311015`.

### Phase 4 — complete (merged to `master` via PR #5)

- `WordNetEngine` now implements `IDisposable`; `Close()` remains as compatibility shim.
- Use-after-dispose guards added for core API calls (`AllWords`, `GetSynSet`, `GetSynSets`, `GetMostCommonSynSet`).
- Disk-mode shared stream access synchronized with locking around shared readers/search streams.
- Broad exceptions replaced with typed exceptions in `WordNetEngine`, `SynSet`, `WordNetSimilarityModel`, and the WinForms harness validation path.
- Robustness tests added in `RobustnessTests` and argument-contract tests expanded in `SimilarityModelTests`.
- Test suite increased to 36/36 passing tests.
- CI green on both runners for PR #5: runs `22825895115` / `22825900637`.

## Current status (Phase 5 — active on `feature/phase-5`)

### Goal

Migrate the legacy project files to SDK-style while preserving behavior and compatibility. Keep the characterization suite as the safety net and avoid public API changes during migration.

### Scope (from `docs/modernization-plan.md`)

| Task | Risk |
|---|---|
| Convert legacy `.csproj` files to SDK-style (`WordNet`, `TestApplication`) | Medium |
| Preserve framework compatibility (`net48`) during migration step | Medium |
| Keep CI/test matrix stable while project format changes | Low–medium |
| Validate no behavior regressions with full characterization suite | Low |

### Done

- [x] Phase 4 PR [#5](https://github.com/CabaVarga/WordNetAPI/pull/5) merged to `master`.
- [x] Branch `feature/phase-5` created from updated `master`.
- [x] Baseline status on branch-open confirmed: working tree clean, 36/36 tests passing at prior phase close.

### Pending

- [ ] Convert `src/WordNet/WordNet.csproj` to SDK-style (`net48`) while preserving assembly/output behavior.
- [ ] Convert `src/TestApplication/TestApplication.csproj` to SDK-style (`net48`, WinForms) while preserving resource/designer behavior.
- [ ] Clean `src/WordNet.sln` legacy configuration noise and remove obsolete net40 CI/workaround plumbing.
- [ ] Re-run full restore/build/test validation and update handoff documents with final outcomes.

## Recommended immediate next steps

1. Convert `WordNet.csproj` to SDK-style first, preserving assembly metadata compatibility.
2. Convert `TestApplication.csproj` to SDK-style WinForms, then clean up `.sln` and CI net40-only steps.
3. Run restore/build/test end-to-end and sync `docs/handoff.md` + `docs/handoff-archive.md` with results.

## Quick restart prompt (for new chat)

```text
Use D:\WordNetAPI-fork on branch feature/phase-5.
Read docs/handoff.md, docs/modernization-plan.md.
Phases 0–4 are merged to master. Phase 5 is active — working tree is clean.
36/36 tests pass. Do not push until instructed.
Phase 4 is merged via PR #5. Continue implementation on `feature/phase-5`.

WORKFLOW REMINDER: When opening a new branch, add a new entry to docs/handoff-archive.md
as the first or second commit, before any implementation work. Record the trigger, references
(PR, CI run, test count), changes made to handoff.md, and any complications from the prior session.
```
