# WordNetAPI Modernization Plan

Date: 2026-03-07  
Source: `docs/quick-repo-audit.md`, `docs/lair-dependencies.md`

## Status snapshot - 2026-03-08 (updated 2026-03-08, session 7)

| Phase | Status |
|---|---|
| Phase 0 — Baseline and Guardrails | ✓ Complete (merged PR #1) |
| Phase 1 — Characterization Tests | ✓ Complete (merged PR #2) |
| Phase 2 — Runtime Side-Effect Hardening | ✓ Complete (merged PR #3) |
| Phase 3 — Dependency Reproducibility | 🔄 In progress (`feature/phase-3`) |
| Phase 3A — LAIR Extraction | 🔄 In progress (`feature/phase-3`, overlaps Phase 3) |
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

## Phase 1 - Characterization Tests ✓ COMPLETE (merged PR #2, 2026-03-08)

- [x] SDK-style test project (`src/WordNet.Tests`, `net48`, MSTest) exists and builds.
- [x] Fixture setup: `ClassInitialize` loads `WordNetEngine` in in-memory mode from `resources/`.
- [x] 5 initial tests in `Test1.cs` — all pass locally and in CI.
- [x] 26 tests total across 4 files:
  - `Test1.cs` — 5 smoke tests (synset IDs, most-common synset, case/space normalization, unknown word)
  - `RelationTraversalTests.cs` — hypernym/hyponym counts, pinned direct hypernym IDs, recursive traversal to entity root, synset words/gloss
  - `SimilarityModelTests.cs` — self-similarity=1, dog–cat > dog–car (WuPalmer max), cross-POS returns 0, average strategy bounds
  - `EdgeCaseTests.cs` — empty string, adjective/adverb POS, unknown word, round-trip by ID, high-polysemy verb, POS-unrestricted search
- [x] Shared `TestHelpers.FindResourcesDirectory()` helper factored out of `Test1.cs`.
- [x] `dotnet test` step added to `.github/workflows/ci-build.yml` (Release config, `--no-build`, after Build).
- [x] All 26 tests pass in CI on both `windows-2022` and `windows-2025`.

**Acceptance criteria met:**

- 26 deterministic tests pass in CI (exceeds 15–25 target).
- Tests pin current behavior for both common and edge-case inputs.

## Phase 2 - Runtime Side-Effect Hardening ✓ COMPLETE (merged PR #3, 2026-03-08)

Address index-file mutation in `WordNetEngine` constructor.

- [x] Introduce explicit preprocessing mode/tool for sorting index files: `WordNetEngine.SortIndexFiles(wordNetDirectory)`.
- [x] Remove implicit write/mutate behavior from normal engine initialization.
- [x] Fail fast with clear `InvalidOperationException` if `.sorted_for_dot_net` marker is absent.
- [x] Add tests for:
  - [x] Sorted dataset path — engine constructs without modifying any index file (`Constructor_SortedDirectory_DoesNotModifyIndexFiles`).
  - [x] Unsorted dataset path — constructor throws `InvalidOperationException` (`Constructor_UnsortedDirectory_ThrowsInvalidOperationException`).
- [x] `.gitattributes` added: `resources/data.*` and `resources/index.*` marked as binary (fixes CRLF corruption of byte offsets on Windows CI).
- [x] 28/28 tests pass in CI on both runners.

**Acceptance criteria met:**

- Normal runtime reads data only; no file rewrite occurs.
- Behavior is explicit and documented.

## Phase 3 + 3A - Dependency Reproducibility / LAIR Extraction ← ACTIVE on `feature/phase-3`

Stabilize `LAIR.*` dependency story by inlining/replacing the minimal needed functionality (Option A from `docs/lair-dependencies.md`). Phases 3 and 3A are being executed together on the same branch.

**Chosen strategy: Option A — internal compatibility layer, then swap internals.**

- [x] **A1 — Remove `LAIR.Extensions`** (low risk): ✓ complete
  - [x] Replace `EnsureContainsKey(...)` (6 sites in `SynSet.cs`, 1 in `WordNetEngine.cs`) with explicit `ContainsKey` + assignment.
  - [x] Replace `TryReadLine(...)` loops with `ReadLine()` null-check loops (6 sites in `WordNetEngine.cs`) + explicit `Close()`.
  - [x] Replace `SetPosition(0)` with `DiscardBufferedData(); BaseStream.Position = 0` (`WordNetEngine.AllWords` disk branch).
- [x] **A2 — Replace `LAIR.IO.BinarySearchTextStream`** (medium risk): ✓ complete
  - [x] Implement internal `BinarySearchTextStream` in `src/WordNet/Internal/`.
  - [x] Remove `using LAIR.IO;` from `WordNetEngine.cs` — type resolves to internal class; no other code changes needed.
- [x] **A3.1 — Introduce internal `Set<T>` shim** (medium-high risk): ✓ complete
  - [x] Add `src/WordNet/Internal/Set.cs` preserving used members: `AddRange`, `IsReadOnly` setter, `new Set<T>()`, `new Set<T>(capacity)`, `new Set<T>(ICollection<T>)`, `new Set<T>(bool)`.
  - [x] No code changes in `SynSet.cs` or `WordNetEngine.cs` — `using LAIR.Collections.Generic;` resolves to the shim.
- [x] Remove `LAIR.*` references from `WordNet.csproj`; confirm clean build.
- [x] Remove `LAIR.*` references from `TestApplication.csproj` and `WordNet.Tests.csproj`.
- [x] Validate with 28/28 characterization tests (both in-memory and disk-mode paths).
- [ ] Add dependency provenance note to docs.

**Acceptance criteria**

- `src/WordNet/WordNet.csproj` has no `LAIR.*` references.
- Build succeeds from a clean checkout using documented steps only.
- 28/28 characterization tests pass in CI on both runners.
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

1. **[Phase 3 — active]** Add dependency provenance note to docs.
2. Open PR for Phase 3 when instructed.
3. Begin Phase 4 (API Robustness — `IDisposable`, defensive validation, typed exceptions).
