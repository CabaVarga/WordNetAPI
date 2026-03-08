# WordNetAPI Handoff

Date: 2026-03-08 (updated 2026-03-08, session 3)

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

## Current status (Phase 2 — complete, pending PR)

### Goal

Remove the implicit index-file mutation that occurs in the `WordNetEngine` constructor. Normal runtime must read data only; sorting must become an explicit preprocessing step.

### Done

- [x] Branch `feature/phase-2` created from updated `master` (post PR #2 merge).
- [x] All Phase 1 carry-overs committed (commit `682abc7`) — working tree is clean.
- [x] 26/26 tests confirmed passing before Phase 2 work began.
- [x] Phase 2 implementation complete:
  - `src/WordNet/WordNetEngine.cs` — extracted sort block into `public static SortIndexFiles(string wordNetDirectory)`; constructor now throws `InvalidOperationException` when `.sorted_for_dot_net` marker is absent.
  - `src/WordNet.Tests/PreprocessingTests.cs` — 2 new tests: `Constructor_SortedDirectory_DoesNotModifyIndexFiles` and `Constructor_UnsortedDirectory_ThrowsInvalidOperationException`.
  - Docs updated: `handoff.md`, `modernization-plan.md`.
- [x] 28/28 tests pass locally (Release config).
- [ ] PR not yet created (do not push until instructed).

## Recommended immediate next steps

1. **Open PR #3** from `feature/phase-2` → `master` when instructed.
2. **Phase 3** — decide `LAIR.*` dependency strategy (commit binaries, replace with NuGet, or inline).
3. **Phase 3A** — begin LAIR extraction with `LAIR.Extensions` replacements (lowest risk).

See `docs/modernization-plan.md` for the full phase checklist.

## Quick restart prompt (for new chat)

```text
Use D:\WordNetAPI-fork on branch feature/phase-2.
Read docs/handoff.md and docs/modernization-plan.md.
Phases 0 and 1 are merged to master. Phase 2 is complete (not yet PR'd).
28/28 tests pass. Do not push until instructed.
Phase 2 summary: WordNetEngine.SortIndexFiles() extracted; constructor now throws InvalidOperationException
if .sorted_for_dot_net marker is absent; PreprocessingTests.cs adds 2 new tests covering both paths.
Next: open PR #3 when instructed, then start Phase 3 (LAIR dependency strategy).
```
