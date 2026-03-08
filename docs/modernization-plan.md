# WordNetAPI Modernization Plan

Date: 2026-03-07  
Source: `docs/quick-repo-audit.md`, `docs/lair-dependencies.md`

## Status snapshot - 2026-03-08

| Phase | Status |
|---|---|
| Phase 0 — Baseline and Guardrails | ✓ Complete (merged PR #1) |
| Phase 1 — Characterization Tests | 🔄 In progress (`feature/phase-1`) |
| Phase 2 — Runtime Side-Effect Hardening | Pending |
| Phase 3 — Dependency Reproducibility | Pending |
| Phase 3A — LAIR Extraction | Pending |
| Phase 4 — API Robustness | Pending |
| Phase 5 — Project System Migration | Pending |
| Phase 6 — Forward Port | Optional |

### Completed findings and decisions

- [x] Baseline buildability validated on modern SDK (`dotnet 9`), including clean rebuild.
- [x] Known warning identified and tracked: missing `AllRules.ruleset` (`MSB3884`) — resolved.
- [x] Runtime mutation risk documented: engine rewrites `index.*` when marker file is missing.
- [x] Dependency inventory completed for `LAIR.*` with concrete usage map across projects.
- [x] Initial dependency direction defined: remove LAIR from core in staged order (`LAIR.Extensions` → `LAIR.IO` → `LAIR.Collections`).
- [x] `LAIR.Extensions` replacement is low risk and can be done first.
- [x] `LAIR.IO.BinarySearchTextStream` replacement is medium risk and should be validated with disk-mode tests.
- [x] `LAIR.Collections.Set<T>` replacement is highest risk and should use a compatibility shim before any public API cleanup.

## Goals

- Preserve existing behavior while reducing modernization risk.
- Make builds reproducible on clean machines/CI.
- Remove hidden runtime side effects.
- Establish regression confidence before framework migration.

## Guiding Principles

- Prefer small, reversible changes.
- Add tests before changing behavior.
- Keep legacy API surface stable until replacement layers are ready.
- Treat `resources/` data as immutable unless an explicit preprocessing step is invoked.

## Phase 0 - Baseline and Guardrails ✓ COMPLETE (merged PR #1, 2026-03-08)

- [x] Capture a reproducible baseline build command in docs.
- [x] Add a minimal CI job: restore/build only (no tests yet).
- [x] Resolve `AllRules.ruleset` warning: removed dangling `<CodeAnalysisRuleSet>` from both legacy project files.
- [x] Document required local prerequisites and data location assumptions in `README.md`.

**Acceptance criteria met:**

- Build is clean (0 warnings, 0 errors) locally and in CI.
- CI reports green on both `windows-2022` and `windows-2025` (run `22820942946`).
- `README.md` covers prerequisites, build commands, and `resources/` layout.

## Phase 1 - Characterization Tests (3-5 days) ← ACTIVE on `feature/phase-1`

### Phase 1 Status - 2026-03-08

- [x] SDK-style test project (`src/WordNet.Tests`, `net48`, MSTest) exists and builds.
- [x] Fixture setup: `ClassInitialize` loads `WordNetEngine` in in-memory mode from `resources/`.
- [x] 5 initial tests in `Test1.cs`:
  - `GetSynSets_DogNoun_ReturnsExpectedIDs`
  - `GetMostCommonSynSet_DogNoun_ReturnsExpectedID`
  - `GetMostCommonSynSet_RunVerb_ReturnsExpectedID`
  - `GetSynSets_NormalizesCaseAndSpaces`
  - `GetSynSets_UnknownWord_ReturnsEmptySet`
- [ ] Confirm all 5 tests pass locally against real `resources/` data.
- [ ] Add characterization tests for:
  - [ ] Synset relation traversal (`GetRelatedSynSets`, hypernyms/hyponyms)
  - [ ] Similarity outputs (`WordNetSimilarityModel`, representative word pairs)
  - [ ] Edge cases (empty string, POS with no entries, very common word)
- [ ] Snapshot canonical word/POS outputs to detect future regressions.
- [ ] Add `dotnet test` step to `.github/workflows/ci-build.yml`.

**Acceptance criteria**

- At least 15–25 deterministic tests pass in CI.
- Tests pin current behavior for both common and edge-case inputs.

## Phase 2 - Runtime Side-Effect Hardening (2-4 days)

Address index-file mutation in `WordNetEngine` constructor.

- [ ] Introduce explicit preprocessing mode/tool for sorting index files.
- [ ] Remove implicit write/mutate behavior from normal engine initialization.
- [ ] Fail fast with clear error if unsorted data is detected and preprocessing was not run.
- [ ] Add tests for:
  - [ ] Sorted dataset path (success)
  - [ ] Unsorted dataset path (predictable failure or explicit preprocess requirement)

**Acceptance criteria**

- Normal runtime reads data only; no file rewrite occurs.
- Behavior is explicit and documented.

## Phase 3 - Dependency Reproducibility (2-3 days)

Stabilize `LAIR.*` dependency story.

- [ ] Choose one strategy:
  - [ ] Commit required binaries in a controlled location and validate paths.
  - [ ] Replace with package-based references.
  - [ ] Inline/replace minimal needed functionality to remove external dependency.
- [ ] Ensure clean clone build works without machine-specific state.
- [ ] Add dependency provenance note (where dependencies come from and why).

**Acceptance criteria**

- Build succeeds from a clean checkout using documented steps only.
- No hidden GAC/local-machine assumptions.

## Phase 3A - LAIR Extraction Track (3-6 days, can overlap Phases 1-4)

Translate findings from `docs/lair-dependencies.md` into executable tasks for removing `LAIR.*` from core library code with controlled risk.

- [ ] Remove `LAIR.Extensions` usage from `WordNet` code paths:
  - [ ] Replace `EnsureContainsKey(...)` with explicit dictionary initialization.
  - [ ] Replace `TryReadLine(...)` with `ReadLine()` null-check loops.
  - [ ] Replace `SetPosition(...)` with buffered stream reset pattern.
- [ ] Replace `LAIR.IO.BinarySearchTextStream` in disk-mode index lookup with internal helper.
- [ ] Introduce an internal `Set<T>` compatibility shim to preserve current behavior (`AddRange`, read-only semantics, constructors in use).
- [ ] Remove `LAIR.*` references from `WordNet.csproj` once replacements are in place.
- [ ] Validate changes with characterization tests before touching `TestApplication` and test-project references.

**Acceptance criteria**

- `src/WordNet/WordNet.csproj` has no `LAIR.*` references.
- Characterization tests pass for both in-memory and disk-mode representative cases.
- Public behavior remains stable for existing consumer-facing flows.

## Phase 4 - API Robustness and Lifetime Management (3-4 days)

- [ ] Implement `IDisposable` on `WordNetEngine` (keep `Close()` as compatibility shim).
- [ ] Add defensive argument validation and typed exceptions where currently broad `Exception` is thrown.
- [ ] Audit disk-mode shared stream access and document single-threaded requirement or add locking.
- [ ] Add tests for disposal behavior and failure contracts.

**Acceptance criteria**

- Consumers can safely and predictably manage engine lifetime.
- Error conditions are clearer and more actionable.

## Phase 5 - Project System Migration (3-5 days)

- [ ] Convert legacy csproj files to SDK-style.
- [ ] Pick an intermediate target (`net48`) as compatibility step.
- [ ] Migrate test project and CI config accordingly.
- [ ] Keep public API and behavior unchanged (validated by characterization tests).

**Acceptance criteria**

- SDK-style projects build locally and in CI.
- Existing consumers can still use the library (or have a documented migration path).

## Phase 6 - Optional Forward Port to Modern .NET (4-8 days)

Only after previous phases are green.

- [ ] Evaluate multi-targeting (e.g., `net48` + modern target) if dependency constraints allow.
- [ ] Resolve compatibility issues (WinForms sample isolation, API differences).
- [ ] Run full test suite across all targets.

**Acceptance criteria**

- Library builds and passes tests on selected modern target(s).
- Release notes document compatibility and known constraints.

## Backlog (Nice-to-Have)

- [ ] Replace WinForms test harness with CLI smoke tool or integration tests.
- [ ] Add performance benchmarks for in-memory vs disk mode.
- [ ] Add structured logging hooks for diagnostics.
- [ ] Improve XML docs and usage examples.

## Estimated Timeline

- **Conservative path (recommended):** 3-5 weeks part-time
- **Focused path:** 2-3 weeks if scope is limited to Phases 0-5 and dependencies are straightforward

## Suggested Next 3 Tasks (Start Here)

1. Create test project and add first 5 characterization tests around `GetSynSets` and `GetMostCommonSynSet`.
2. Decide dependency strategy for `LAIR.*` and document the chosen approach.
3. Refactor index sorting into an explicit preprocessing step (no implicit mutation on constructor).
