# WordNetAPI Handoff

Date: 2026-03-08 (updated 2026-03-08, session 8)

## Current working context

- Local repo: `D:\WordNetAPI-fork`
- Active branch: `feature/phase-4`
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

## Current status (Phase 4 — active on `feature/phase-4`)

### Goal

Improve API robustness and lifetime management. Consumers should be able to safely and predictably manage engine lifetime. Error conditions should be clearer and more actionable.

### Scope (from `docs/modernization-plan.md`)

| Task | Risk |
|---|---|
| Implement `IDisposable` on `WordNetEngine` (keep `Close()` as compatibility shim) | Low–medium |
| Add defensive argument validation and typed exceptions where currently broad `Exception` is thrown | Low |
| Audit disk-mode shared stream access and document single-threaded requirement or add locking | Medium |
| Add tests for disposal behavior and failure contracts | Low |

### Done

- [x] Branch `feature/phase-4` created from updated `master` (post PR #4 merge).
- [x] 28/28 tests confirmed passing on `master`.

### Pending

- [ ] Implement `IDisposable` on `WordNetEngine`; wire `Close()` to call `Dispose()`.
- [ ] Replace broad `throw new Exception(...)` with typed exceptions (`ArgumentException`, `InvalidOperationException`, `FormatException`, etc.) across `SynSet.cs` and `WordNetEngine.cs`.
- [ ] Audit disk-mode stream access for thread safety; document or add locking.
- [ ] Add tests for disposal (double-dispose, use-after-dispose) and typed exception contracts.

## Recommended immediate next steps

1. Audit `WordNetEngine.cs` and `SynSet.cs` for all `throw new Exception(...)` sites — catalog them.
2. Implement `IDisposable` on `WordNetEngine`; add disposal tests.
3. Replace broad exceptions with typed ones; add contract tests.

## Quick restart prompt (for new chat)

```text
Use D:\WordNetAPI-fork on branch feature/phase-4.
Read docs/handoff.md, docs/modernization-plan.md.
Phases 0–3 are merged to master. Phase 4 is active — working tree is clean.
28/28 tests pass. Do not push until instructed.
Phase 4 goal: API robustness — IDisposable, typed exceptions, thread-safety audit.

WORKFLOW REMINDER: When opening a new branch, add a new entry to docs/handoff-archive.md
as the first or second commit, before any implementation work. Record the trigger, references
(PR, CI run, test count), changes made to handoff.md, and any complications from the prior session.
```
