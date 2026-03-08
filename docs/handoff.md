# WordNetAPI Handoff

Date: 2026-03-08 (updated 2026-03-08, session 2)

## Current working context

- Local repo: `D:\WordNetAPI-fork`
- Active branch: `feature/phase-2`
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
  - Relation traversal: hypernym/hyponym counts, pinned direct hypernym IDs, recursive traversal to entity root, synset words/gloss.
  - Similarity model: self-similarity=1, dog–cat > dog–car (WuPalmer max), cross-POS returns 0, average strategy bounds.
  - Edge cases: empty string, adjective/adverb POS, unknown word, round-trip by ID, high-polysemy verb, POS-unrestricted search.
- `dotnet test` step added to `.github/workflows/ci-build.yml` (Release config, `--no-build`, after Build).
- Shared `TestHelpers.FindResourcesDirectory()` helper factored out of `Test1.cs`.
- CI green on both runners.

## Current status (Phase 2)

### Goal

Remove the implicit index-file mutation that occurs in the `WordNetEngine` constructor. Normal runtime must read data only; sorting must become an explicit preprocessing step.

### Done

- [x] Branch `feature/phase-2` created from updated `master` (post PR #2 merge).
- [x] All Phase 1 carry-overs committed (commit `682abc7`) — working tree is clean:
  - `.github/workflows/ci-build.yml` — `dotnet test` step
  - `src/WordNet.Tests/Test1.cs` — `FindResourcesDirectory()` delegates to `TestHelpers`
  - `src/WordNet.Tests/TestHelpers.cs` — shared helper (was never committed to any branch)
  - `src/WordNet.Tests/RelationTraversalTests.cs`, `SimilarityModelTests.cs`, `EdgeCaseTests.cs` — same
  - Docs updated: `handoff.md`, `modernization-plan.md`, `fork-plan.md`, `quick-repo-audit.md`
- [x] 26/26 tests confirmed passing locally (Release config).
- [x] Branch pushed to `origin/feature/phase-2`.

### Pending

- [ ] Audit `WordNetEngine` constructor: locate and isolate the index-sorting write path.
- [ ] Introduce an explicit preprocessing mode/tool that sorts the index files on demand.
- [ ] Remove the implicit write/mutate behavior from normal engine initialization.
- [ ] Add a clear fail-fast error when unsorted data is detected and preprocessing has not been run.
- [ ] Add tests:
  - [ ] Sorted dataset path → engine loads successfully, no file writes.
  - [ ] Unsorted dataset path → predictable failure or explicit preprocess requirement.

## Recommended immediate next steps

1. **Audit the constructor** — find the sorting and file-write code in `WordNetEngine`.
2. **Extract** the sorting logic into a standalone `Preprocess` / `SortIndexFiles` method or CLI entry point.
3. **Guard** normal init: detect unsorted state, throw a descriptive exception if preprocessing has not been run.
4. **Add tests** covering both paths before touching any behavior.

See `docs/modernization-plan.md` Phase 2 for the full checklist and acceptance criteria.

## Quick restart prompt (for new chat)

```text
Use D:\WordNetAPI-fork on branch feature/phase-2.
Read docs/handoff.md, docs/modernization-plan.md, and docs/lair-dependencies.md.
Phases 0 and 1 are merged to master. Phase 2 is active — working tree is clean, all carry-overs committed.
26/26 tests pass. Branch is pushed to origin/feature/phase-2.
Phase 2 goal: remove implicit index-file mutation from WordNetEngine constructor.
Extract sorting into an explicit preprocessing step; add fail-fast guard; add tests for both paths.
Start by auditing the WordNetEngine constructor for the index-sorting/file-write path.
```
