# WordNetAPI Modernization Plan

Date: 2026-03-07  
Source: `docs/quick-repo-audit.md`

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

## Phase 0 - Baseline and Guardrails (1-2 days)

- [ ] Capture a reproducible baseline build command in docs.
- [ ] Add a minimal CI job: restore/build only (no tests yet).
- [ ] Track known warnings (currently missing `AllRules.ruleset`) and decide: add file vs remove setting.
- [ ] Document required local prerequisites and data location assumptions.

**Acceptance criteria**

- A fresh environment can run one documented build command successfully.
- CI reports green build status on `master`.

## Phase 1 - Characterization Tests (3-5 days)

Create a new test project (SDK-style is fine) that references the existing library assembly/project.

- [ ] Add fixture setup for loading `resources/` test data.
- [ ] Add characterization tests for:
  - [ ] `GetSynSets(word, pos)`
  - [ ] `GetMostCommonSynSet(word, pos)`
  - [ ] Synset relation traversal (`GetRelatedSynSets`, shortest path/depth)
  - [ ] Similarity outputs (`WordNetSimilarityModel`, representative cases)
- [ ] Snapshot a small set of canonical word/POS outputs to detect regressions.
- [ ] Add CI test execution.

**Acceptance criteria**

- At least 15-25 deterministic tests pass in CI.
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
