# WordNetAPI Handoff

Date: 2026-03-08

## Current working context

- Local repo: `D:\WordNetAPI-fork`
- Active branch: `feature/phase-0`
- Fork repo: `https://github.com/CabaVarga/WordNetAPI.git`
- Upstream repo: `https://github.com/zacg/WordNetAPI.git`
- `gh` CLI is installed/authenticated and default repo is set to `CabaVarga/WordNetAPI`.

## Snapshot of work completed

- Added a modernization baseline and phased plan in `docs/modernization-plan.md`.
- Added stop-point snapshot and next-step plan to `docs/modernization-plan.md`.
- Added LAIR dependency analysis in `docs/lair-dependencies.md`.
- Added migration runbook in `docs/fork-plan.md`.
- Added CI workflow at `.github/workflows/ci-build.yml`.
- Diagnosed initial CI failure from GitHub logs (`MSB3644` for missing `.NETFramework,Version=v4.0` reference assemblies on `windows-2025` runner).
- Fixed `WordNet.Tests` LAIR reference paths to be stable (`lib`-based instead of `src/WordNet/bin/Debug`):
  - `src/WordNet.Tests/WordNet.Tests.csproj`
- Added SDK pin file:
  - `global.json` (`9.0.300`)
- Updated CI workflow in two iterations:
  1. Pin SDK + attempt targeting-pack install
  2. Switch to `windows-2019` and remove failing Chocolatey install step
- Confirmed `windows-2019` is not a valid hosted label for current GitHub runner images; queued run stayed blocked until cancelled.
- Updated CI workflow to a matrix for supported images:
  - Required: `windows-2022`
  - Canary (non-blocking): `windows-2025`
- Pushed fix commits:
  - `30a085b` `fix: stabilize CI build environment for legacy projects`
  - `db57e94` `fix: run CI on windows-2019 for net40 targeting`
  - `cdf1eec` `fix: move CI off windows-2019 with 2025 canary`
- **Phase 0 closeout** (`2be89ce` `fix: make CI green for net40 projects on hosted Windows runners`):
  - Added `Directory.Build.props` at repo root: sets `FrameworkPathOverride` from `CI_NETFX40_PATH` env var for `v4.0` projects only; no-op on developer machines.
  - Updated CI workflow: added step to install `Microsoft.NETFramework.ReferenceAssemblies.net40` via `nuget.exe` and export path before `dotnet build`.
  - Removed dangling `<CodeAnalysisRuleSet>AllRules.ruleset</CodeAnalysisRuleSet>` from `WordNet.csproj` and `TestApplication.csproj` (file never existed → `MSB3884` warnings).
  - Fixed `WordNet.csproj` Release `OutputPath` from broken `..\..\..\..\Libraries\` to `bin\Release\`.
  - Updated `README.md` with prerequisites, build commands, data-file layout, project table, CI badge, and modernization roadmap pointer.
  - CI run `22820942946`: both `windows-2022` and `windows-2025` green. ✓

## Current status (Phase 0)

- [x] Baseline build command documented.
- [x] Minimal CI workflow added.
- [x] CI workflow hardened: both `windows-2022` and `windows-2025` pass.
- [x] `AllRules.ruleset` warnings resolved (references removed from both legacy project files).
- [x] `README.md` prerequisites/build guidance expanded.

**Phase 0 is complete.**

## Recommended immediate next step

Start Phase 1 — characterization tests:

1. Create a test fixture that loads WordNet data from `resources/` (or a small test subset).
2. Add 5 deterministic tests for `GetSynSets(word, pos)` and `GetMostCommonSynSet(word, pos)`.
3. Wire test execution into the CI workflow (`dotnet test`).

See `docs/modernization-plan.md` Phase 1 for the full checklist and acceptance criteria.

## Quick restart prompt (for new chat)

```text
Use D:\WordNetAPI-fork on branch feature/phase-0.
Read docs/handoff.md, docs/modernization-plan.md, and docs/lair-dependencies.md.
Phase 0 is complete; CI is green (run 22820942946, both windows-2022 and windows-2025 pass).
Start Phase 1: add a test fixture that loads WordNet resources/ data, then add
characterization tests for GetSynSets and GetMostCommonSynSet in WordNet.Tests.
Wire dotnet test into the CI workflow once the first tests pass locally.
```
