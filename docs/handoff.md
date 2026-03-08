# WordNetAPI Handoff

Date: 2026-03-08

## Current working context

- Local repo: `D:\WordNetAPI-fork`
- Active branch: `feature/phase-1`
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

## Current status (Phase 1)

### Done

- [x] `WordNet.Tests` project exists and builds (`net48`, MSTest).
- [x] Test fixture (`ClassInitialize`) loads `WordNetEngine` in in-memory mode from `resources/`.
- [x] 5 initial tests in `Test1.cs`:
  - `GetSynSets_DogNoun_ReturnsExpectedIDs` — pins all synset IDs for "dog" noun.
  - `GetMostCommonSynSet_DogNoun_ReturnsExpectedID` — pins most-common synset.
  - `GetMostCommonSynSet_RunVerb_ReturnsExpectedID` — pins most-common synset for a verb.
  - `GetSynSets_NormalizesCaseAndSpaces` — verifies "new york" / "New York" equivalence.
  - `GetSynSets_UnknownWord_ReturnsEmptySet` — verifies unknown words return empty.

### Pending

- [ ] Verify tests pass locally against the real `resources/` data (requires WordNet 3.1 data files).
- [ ] Expand to 15–25 tests covering: synset relation traversal, similarity model representative cases, edge cases.
- [ ] Add `dotnet test` step to the CI workflow.
- [ ] Snapshot canonical outputs for regression detection.

## Recommended immediate next steps

1. **Run existing tests locally** to confirm the 5 tests in `Test1.cs` pass against your `resources/` directory.
   ```powershell
   dotnet test src/WordNet.sln --configuration Debug
   ```
2. **Expand test coverage** — add tests for:
   - Synset relation traversal (`GetRelatedSynSets`, hypernyms/hyponyms).
   - `WordNetSimilarityModel` (pick 2–3 representative word pairs with known expected scores).
   - Boundary cases: empty string, POS with no entries, very common word.
3. **Wire `dotnet test` into CI** once the local suite is stable (add a `test` step after `build` in `ci-build.yml`).

See `docs/modernization-plan.md` Phase 1 for the full checklist and acceptance criteria.

## Quick restart prompt (for new chat)

```text
Use D:\WordNetAPI-fork on branch feature/phase-1.
Read docs/handoff.md, docs/modernization-plan.md, and docs/lair-dependencies.md.
Phase 0 is merged to master. Phase 1 is in progress.
WordNet.Tests already has 5 smoke tests in src/WordNet.Tests/Test1.cs.
First, run `dotnet test src/WordNet.sln` locally and confirm all 5 tests pass.
Then expand coverage to 15-25 tests (relation traversal, similarity model, edge cases)
and add a dotnet test step to .github/workflows/ci-build.yml.
```
