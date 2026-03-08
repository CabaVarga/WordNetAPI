# WordNetAPI Handoff

Date: 2026-03-08 (updated 2026-03-08)

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
- [x] Two carry-over changes staged (rescued from dropped stash — NOT yet committed):
  - `.github/workflows/ci-build.yml` — `dotnet test` step (was in stash, not included in PR #2)
  - `src/WordNet.Tests/Test1.cs` — `FindResourcesDirectory()` delegates to `TestHelpers` (same)

### Pending

- [ ] **Commit the two staged carry-over files** before starting any Phase 2 work.
- [ ] Audit `WordNetEngine` constructor: locate and isolate the index-sorting write path.
- [ ] Introduce an explicit preprocessing mode/tool that sorts the index files on demand.
- [ ] Remove the implicit write/mutate behavior from normal engine initialization.
- [ ] Add a clear fail-fast error when unsorted data is detected and preprocessing has not been run.
- [ ] Add tests:
  - [ ] Sorted dataset path → engine loads successfully, no file writes.
  - [ ] Unsorted dataset path → predictable failure or explicit preprocess requirement.

## Recommended immediate next steps

1. **Commit the staged carry-over files** — confirm with `git status`, then commit.
   ```powershell
   git status   # should show .github/workflows/ci-build.yml and src/WordNet.Tests/Test1.cs staged
   git add .github/workflows/ci-build.yml src/WordNet.Tests/Test1.cs
   git commit -m "chore: carry over ci test step and Test1 TestHelpers delegation from phase-1"
   dotnet test src/WordNet.sln --configuration Release   # confirm 26/26 still pass
   ```
2. **Audit the constructor** — find the sorting and file-write code in `WordNetEngine`.
3. **Extract** the sorting logic into a standalone `Preprocess` / `SortIndexFiles` method or CLI entry point.
4. **Guard** normal init: detect unsorted state, throw a descriptive exception if preprocessing has not been run.
5. **Add tests** covering both paths before touching any behavior.

See `docs/modernization-plan.md` Phase 2 for the full checklist and acceptance criteria.

## Quick restart prompt (for new chat)

```text
Use D:\WordNetAPI-fork on branch feature/phase-2.
Read docs/handoff.md, docs/modernization-plan.md, and docs/lair-dependencies.md.
Phases 0 and 1 are merged to master. Phase 2 is active (not yet started).
26 tests pass across Test1.cs, RelationTraversalTests.cs, SimilarityModelTests.cs, EdgeCaseTests.cs.
FIRST: two staged carry-over files need committing before any Phase 2 work —
  .github/workflows/ci-build.yml (dotnet test step) and src/WordNet.Tests/Test1.cs (TestHelpers delegation).
Run `git status` to confirm, commit them, then `dotnet test` to verify 26/26 still pass.
Phase 2 goal: remove implicit index-file mutation from WordNetEngine constructor.
Extract sorting into an explicit preprocessing step; add fail-fast guard; add tests for both paths.
```
