# Quick Repository Audit

Date: 2026-03-07  
Repository: `WordNetAPI`

## Scope

This is a fast audit focused on:

- Repository structure and project layout
- Buildability on a modern .NET SDK
- Dependency/reproducibility risk
- Maintainability and modernization risk
- Testing and operational readiness

## Snapshot

- Solution: `src/WordNet.sln` (Visual Studio 2010 format)
- Projects:
  - `src/WordNet/WordNet.csproj` (`TargetFrameworkVersion` = `v4.0`, library)
  - `src/TestApplication/TestApplication.csproj` (`TargetFrameworkVersion` = `v4.0`, WinForms app)
- Core code files in library:
  - `src/WordNet/WordNetEngine.cs`
  - `src/WordNet/SynSet.cs`
  - `src/WordNet/WordNetSimilarityModel.cs`
- Resource payload present under `resources/` (WordNet data + `.sorted_for_dot_net` marker)

## What Was Verified

- `dotnet --version` returns `9.0.300`
- `dotnet build src/WordNet.sln` succeeds
- `dotnet clean && dotnet build` also succeeds
- Build warnings:
  - Missing `AllRules.ruleset` in both projects (`MSB3884`)

## Findings (Prioritized)

### 1) Runtime mutates upstream WordNet index files (High)

`WordNetEngine` sorts and rewrites `index.*` files on startup if `.sorted_for_dot_net` is missing, then drops a marker file.  
Impact:

- First run has side effects on data at rest
- Same dataset may not stay compatible across different tooling/runtimes
- Unsafe when dataset is shared, mounted read-only, or managed externally

### 2) Dependency reproducibility is fragile (High)

Projects reference legacy `LAIR.*` assemblies via HintPath (`..\..\lib\...dll`), but repo `lib/` currently contains XML docs only.  
Impact:

- Builds can succeed/fail depending on machine state (GAC/local environment/previous artifacts)
- Onboarding and CI reproducibility risk is high

### 3) No automated test project or CI test signal (High) — ✓ RESOLVED (Phase 1, PR #2)

No unit/integration test project detected; validation is primarily a manual WinForms test harness (`TestApplication`).  
Impact:

- Refactor/upgrade risk is high
- Behavioral regressions are likely during modernization

**Resolution:** `src/WordNet.Tests` (SDK-style, `net48`, MSTest) added with 26 characterization tests covering synset retrieval, relation traversal, similarity model, and edge cases. `dotnet test` step wired into CI; all 26 tests pass on `windows-2022` and `windows-2025`.

### 4) Legacy project system and framework target (Medium)

Both projects are pre-SDK-style MSBuild with `.NET Framework 4.0` metadata and VS2010 solution format.  
Impact:

- Modern tooling friction
- Harder package/dependency management
- Limits multi-platform and long-term support options

### 5) API lifetime and thread-safety risk (Medium)

`WordNetEngine` uses manual `Close()` instead of `IDisposable`; disk-mode uses shared `StreamReader` instances for synset data access without explicit synchronization.  
Impact:

- Resource leak risk if callers forget `Close()`
- Potential race conditions under concurrent access

### 6) Error handling style is brittle for library consumers (Medium)

Core paths throw broad `Exception`/`NullReferenceException` in many validation and parsing flows.  
Impact:

- Harder for callers to classify/recover from failures
- Operational diagnostics quality is lower than typed exception patterns

### 7) Local-machine assumptions in sample app (Low)

`TestApplication` contains hardcoded path logic (`...\dev\wordnetapi\resources\`) in UI startup.  
Impact:

- Sample app is not portable as-is
- Confusing first-run behavior for new contributors

## Quick Wins (1-2 days)

1. ✓ Replace missing ruleset references or add the expected `AllRules.ruleset` file. *(done — Phase 0)*
2. ✓ Add a small SDK-style test project (even if production code remains legacy) with characterization tests for: *(done — Phase 1)*
   - `GetSynSets`
   - `GetMostCommonSynSet`
   - Synset relation traversal
   - Similarity calculations
3. Make sample-app data path configurable (app config, env var, or file picker).
4. ✓ Add a short contributor doc for required data files and dependency expectations. *(done — Phase 0, `README.md`)*

## Recommended Modernization Sequence

1. **Stabilize behavior first**
   - Add characterization tests against current outputs using the checked-in `resources/`.
2. **Harden runtime behavior**
   - Remove/guard implicit index-file mutation (option flag or explicit one-time preprocessing tool).
3. **Fix reproducibility**
   - Decide dependency strategy for `LAIR.*` (vendor binaries, migrate to package refs, or replace APIs).
4. **Improve API robustness**
   - Introduce `IDisposable` pattern, typed exceptions, and clear failure contracts.
5. **Migrate project format**
   - Move to SDK-style `.csproj` and evaluate target framework path (`net48` as stepping stone, then modern .NET if feasible).

## Overall Assessment

The codebase is still buildable and functionally coherent, but modernization risk is **moderate-to-high** due to reproducibility gaps, runtime side effects on data files, and absence of automated regression coverage.  
The safest next step is to establish characterization tests before refactoring or framework migration.
