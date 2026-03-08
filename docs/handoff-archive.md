# WordNetAPI Handoff Archive

Chronological record of each `docs/handoff.md` revision, newest first.
Each entry summarises what changed relative to the prior version.

---

## Methodology

### Purpose

This file is the longitudinal companion to `handoff.md`. Where `handoff.md` always
reflects the **current** state, this file preserves a human-readable record of how
the project evolved session by session — capturing not just what was done but why the
handoff document changed and what complications arose.

### When to update

- **At the start of a new branch**, as the first or second commit, before any
  implementation work begins. Writing the entry while the previous session is fresh
  produces more accurate trigger and complication notes than reconstructing later.
- **At the end of a session** if a mid-session complication changes the handoff
  significantly (e.g. a carry-over situation, a CI hotfix, a scope change).

### Entry format

Each entry must contain:

```
## Session <label> — <date> · `<short commit>` · branch `<branch>`

**Trigger:** <one sentence: what caused a new handoff.md revision>

**References:** PR #N · CI run `<id>` · <N> tests at close

**Changes to handoff.md:** <bullet list of what was new or different>

**Complications / deviations:** <bullet list, or "None." if clean>
```

- `Session <label>` uses a number for the primary session and a letter suffix (`a`, `b`)
  when a single working session produced multiple handoff revisions (e.g. branch-open
  then carry-overs-resolved).
- The **References** line anchors each entry to verifiable artefacts (PR, CI run, test
  count). Omit items that do not exist yet (e.g. PR is "pending" at session close).
- **Complications / deviations** is the highest-value field for future sessions. Record
  anything that was unplanned, required correction, or signals a risk for similar work.
- Entries are **append-only** and inserted at the top (below this Methodology section).
  Never edit a past entry except to fix a factual error; note the correction inline.

### Summary table

Keep the table below updated. One row per phase; update when a phase closes (PR merged).

| Phase | Sessions | Closing commit | Tests at close | PR |
|-------|----------|---------------|----------------|----|
| 0 — Baseline and Guardrails       | 1a       | `4735631` | 0 (build-only) | [#1](https://github.com/CabaVarga/WordNetAPI/pull/1) |
| 1 — Characterization Tests        | 1b, 2a   | `682abc7` | 26             | [#2](https://github.com/CabaVarga/WordNetAPI/pull/2) |
| 2 — Runtime Side-Effect Hardening | 2b, 3    | `07c1b68` | 28             | [#3](https://github.com/CabaVarga/WordNetAPI/pull/3) |
| 3 — Dependency Reproducibility    | 4, 5, 6, 7 | `2bd520b` | 28             | [#4](https://github.com/CabaVarga/WordNetAPI/pull/4) |
| 4 — API Robustness                | 8, 9, 10 → | —         | 36             | pending |

---

## Session 10 — 2026-03-08 · `b217675` · branch `feature/phase-4`

**Trigger:** Final Phase 4 wrap-up pass requested before PR opening.

**References:** PR pending · 36 tests at close (local `dotnet test`) · key commits `73587e1`, `b217675`.

**Changes to handoff.md:**

- Date line updated to "session 10".
- Current status promoted from "Phase 4 active" to "Phase 4 complete, pending PR".
- Done list expanded with README wrap-up note (explicit preprocessing contract and
  thread-safety/lifetime guidance).
- Pending reduced to a single actionable item: open Phase 4 PR when instructed.
- Recommended next steps changed to PR-focused sequence and Phase 5 handoff.
- Quick restart prompt updated to describe Phase 4 implementation as complete and PR-pending.

**Complications / deviations:**

- README still contained pre-Phase-2 behavior text ("engine may rewrite index files on first run").
  Corrected during wrap-up to match current explicit preprocessing contract.

---

## Session 9 — 2026-03-08 · `73587e1` · branch `feature/phase-4`

**Trigger:** Core Phase 4 engineering work implemented (`IDisposable`, typed exceptions, disk-mode synchronization).

**References:** PR pending · 36 tests at close (local `dotnet test`) · commits `73587e1`, `b217675`.

**Changes to handoff.md:**

- Date line updated to "session 9".
- Done list expanded with completed Phase 4 engineering outcomes:
  - `WordNetEngine` `IDisposable` support with `Close()` compatibility shim.
  - use-after-dispose guards on core API methods.
  - disk-mode shared-stream locking for read-path synchronization.
  - typed exception migration across `WordNetEngine`, `SynSet`, `WordNetSimilarityModel`,
    and WinForms harness validation path.
  - robustness + argument-contract tests added; total increased to 36/36 passing.
- Pending narrowed to PR-oriented cleanup tasks.
- Recommended next steps shifted from implementation to synchronization and PR prep.

**Complications / deviations:** None.

---

## Session 8 — 2026-03-08 · `d585591` · branch `feature/phase-4`

**Trigger:** Phase 3 merged; branch context switched to Phase 4 kickoff.

**References:** PR [#4](https://github.com/CabaVarga/WordNetAPI/pull/4) merged · 28 tests at branch-open.

**Changes to handoff.md:**

- Date line updated to "session 8".
- Active branch switched from `feature/phase-3` to `feature/phase-4`.
- Phase 3 section reframed as complete/merged and moved into phase history.
- Current status reset to Phase 4 scope (`IDisposable`, typed exceptions, thread-safety audit).
- Pending and recommended steps reset from Phase 3 execution to Phase 4 implementation.
- Quick restart prompt rewritten for Phase 4 branch context and clean-tree expectation.

**Complications / deviations:** None.

---

## Session 7 — 2026-03-08 · `dd2991e` · branch `feature/phase-3`

**Trigger:** Continuation of Phase 3; A3.1 (`Set<T>` shim) implemented; all LAIR refs removed; PR #4 opened and merged.

**References:** PR [#4](https://github.com/CabaVarga/WordNetAPI/pull/4) merged · CI runs `22825302479` / `22825311015` (both runners green) · 28 tests at close.

**Changes to handoff.md:**

- Date line updated to "session 7".
- Done list expanded: A3.1 marked complete — `src/WordNet/Internal/Set.cs` added as public
  `Set<T>` in `LAIR.Collections.Generic` namespace backed by `HashSet<T>`; all three `LAIR.*`
  DLL references removed from `WordNet.csproj`, `TestApplication.csproj`, `WordNet.Tests.csproj`.
  28/28 tests, 0 warnings, 0 errors across all three projects.
- Pending reduced to dependency provenance note (completed same session) and PR.
- Recommended next steps updated: merge PR #4, then Phase 4.
- Quick restart prompt updated: Phase 3 described as complete; points to Phase 4.
- Dependency provenance section added to `docs/lair-dependencies.md`.

**Complications / deviations:**

- TestApplication build initially failed with CS0433 (ambiguous `Set<T>` between LAIR DLL and
  WordNet assembly). Required removing LAIR refs from `TestApplication.csproj` and
  `WordNet.Tests.csproj` in the same commit as the shim, rather than as a separate step.
  The planned sequencing (A3.1 → remove from WordNet.csproj → remove from downstream) was
  collapsed into a single atomic change.

---

## Session 6 — 2026-03-08 · `0fe1656` · branch `feature/phase-3`

**Trigger:** Continuation of Phase 3; A2 (`LAIR.IO.BinarySearchTextStream` replacement) implemented.

**References:** No PR yet · 28 tests at close (local Release config).

**Changes to handoff.md:**

- Date line updated to "session 6".
- Done list expanded: A2 marked complete — `src/WordNet/Internal/BinarySearchTextStream.cs`
  added as internal byte-level binary search; `using LAIR.IO;` removed from `WordNetEngine.cs`;
  `<Compile Include>` added to legacy `WordNet.csproj`. 28/28 tests, 0 warnings, 0 errors.
- Recommended next steps updated: A3.1 is now first (internal `Set<T>` shim), then LAIR
  ref removal, then downstream project cleanup.
- Quick restart prompt updated: A1 and A2 described as complete; directs to continue with A3.1.

**Complications / deviations:** None.

---

## Session 5 — 2026-03-08 · `17c410c` · branch `feature/phase-3`

**Trigger:** Continuation of Phase 3 on `feature/phase-3`; A1 (`LAIR.Extensions` removal) implemented.

**References:** No PR yet · 28 tests at close (local Release config).

**Changes to handoff.md:**

- Date line updated to "session 5".
- Done list expanded: A1 marked complete with full detail — `using LAIR.Extensions;` removed
  from both `SynSet.cs` and `WordNetEngine.cs`; 7 `EnsureContainsKey` → `ContainsKey` + `new`;
  6 `TryReadLine` loops → `ReadLine()` null-check + explicit `Close()`; 1 `SetPosition(0)` →
  `DiscardBufferedData(); BaseStream.Position = 0`. 28/28 tests, 0 warnings, 0 errors.
- Recommended next steps updated: A2 is now first (implement `IndexBinarySearchReader`), then
  A3.1, then LAIR ref removal.
- Quick restart prompt updated: A1 described as complete; directs to continue with A2.

**Complications / deviations:** None.

---

## Session 4 — 2026-03-08 · `c215899` · branch `feature/phase-3`

**Trigger:** PR #3 merged; `master` pulled; `feature/phase-3` created.

**References:** PR [#3](https://github.com/CabaVarga/WordNetAPI/pull/3) merged · CI runs `22822216371` / `22822216813` (both runners green) · 28 tests at close.

**Changes to handoff.md:**

- Active branch updated to `feature/phase-3`.
- Phase 2 promoted into phase history, condensed to a single bullet block; now includes
  the `.gitattributes` hotfix (binary encoding for `resources/data.*` / `index.*`).
- "Current status" section replaced entirely: Phase 3 goal documented — stabilise
  `LAIR.*` dependency story using **Option A** (inline/replace) from `docs/lair-dependencies.md`.
- Staged plan table added: A1 (`LAIR.Extensions`), A2 (`BinarySearchTextStream`), A3.1
  (`Set<T>` shim), with risk ratings for each step.
- Pending checklist reflects the three extraction steps plus downstream project clean-up.
- Recommended next steps updated to A1 first (lowest risk, mechanical substitutions).
- Quick restart prompt updated to `feature/phase-3`; points to A1 as the entry point.

**Complications / deviations:** None.

---

## Session 3 — 2026-03-08 · `6e1d77e` · branch `feature/phase-2`

**Trigger:** Phase 2 implementation complete; PR #3 not yet opened (instructed not to push).

**References:** PR [#3](https://github.com/CabaVarga/WordNetAPI/pull/3) pending · 28 tests at close (local Release config).

**Changes to handoff.md:**

- Date line updated to "session 3".
- "Current status" header changed to "Phase 2 — complete, pending PR".
- Done list condensed: carry-over detail removed; Phase 2 implementation items added:
  - `SortIndexFiles()` extraction and `InvalidOperationException` guard in constructor.
  - `PreprocessingTests.cs` with two new tests.
  - 28/28 test count.
- Pending reduced to a single note: "PR not yet created (do not push until instructed)."
- Recommended next steps replaced: PR #3, then Phase 3 strategy decision, then Phase 3A.
- Quick restart prompt updated: branch still `feature/phase-2`; Phase 2 described as
  complete but not yet pushed; next step is PR #3.

**Complications / deviations:**

- Tests passed 28/28 locally but CI failed on first push with `Position mismatch` errors
  across 23 disk-mode tests. Root cause: no `.gitattributes` meant Windows CI runners
  (`core.autocrlf = true`) checked out `resources/data.*` and `index.*` with CRLF line
  endings, shifting every byte offset. Fix: a second commit (`07c1b68`) added
  `.gitattributes` marking those files as binary. CI green only after that hotfix.
  This entry was written before the hotfix; the hotfix is recorded in the Session 4
  phase-history bullet for Phase 2.

---

## Session 2 — carry-overs resolved — 2026-03-08 · `bd38500` · branch `feature/phase-2`

**Trigger:** All Phase 1 carry-overs committed; working tree confirmed clean; branch pushed.

**References:** No PR yet · 26 tests at close (Release config, local).

**Changes to handoff.md:**

- Date line updated to "session 2".
- Done list expanded to the full list of committed carry-over files
  (`ci-build.yml`, `Test1.cs`, `TestHelpers.cs`, three new test files, docs).
- 26/26 tests confirmed passing (Release config) added to Done.
- "Branch pushed to `origin/feature/phase-2`" added to Done.
- Carry-over commit item removed from Pending; Pending now contains only Phase 2 tasks.
- Recommended next steps simplified: carry-over step removed; starts at "Audit the constructor."
- Quick restart prompt updated: carry-over instruction dropped; working tree described as
  clean; directs to start at constructor audit.

**Complications / deviations:** None (carry-overs were the complication from 2a; this entry records their resolution).

---

## Session 2 — branch open — 2026-03-08 · `682abc7` · branch `feature/phase-2`

**Trigger:** PR #2 merged; `master` pulled; `feature/phase-2` created; carry-over situation discovered.

**References:** PR [#2](https://github.com/CabaVarga/WordNetAPI/pull/2) merged · 26 tests (discovered locally, not yet in CI) · no new CI run.

**Changes to handoff.md:**

- Active branch updated to `feature/phase-2`.
- Phase 1 promoted from "Current status" into "Phase history" (condensed bullet block).
- "Current status" section reset to Phase 2 scope; goal stated for the first time.
- Done list is minimal: branch created; two carry-over files staged but NOT yet committed.
- Pending list leads with a prominent "Commit the staged carry-over files" instruction
  before any Phase 2 technical tasks.
- Recommended next steps: step 1 is an explicit `git status` / `git add` / `git commit` /
  `dotnet test` sequence for the carry-overs; steps 2–5 cover Phase 2 design.
- Quick restart prompt carries the carry-over warning as the most urgent first action.

**Complications / deviations:**

- A stash was dropped between sessions. The Phase 1 `dotnet test` CI step and the
  `Test1.cs` `TestHelpers` delegation had been worked on locally but were never staged
  for PR #2. They were rescued and staged before the first commit on this branch, but
  required a corrective commit at the very start of Phase 2 work. Additionally,
  `TestHelpers.cs`, `RelationTraversalTests.cs`, `SimilarityModelTests.cs`, and
  `EdgeCaseTests.cs` had also never been committed to any branch and were recovered at
  the same time. All five files were committed as a single carry-over commit (`682abc7`)
  before Phase 2 work began.

---

## Session 1b — 2026-03-08 · `296f3fb` · branch `feature/phase-1`

**Trigger:** PR #1 merged; `feature/phase-1` created; initial test suite scaffolded.

**References:** PR [#1](https://github.com/CabaVarga/WordNetAPI/pull/1) merged · CI run `22820942946` (Phase 0 baseline) · 5 tests scaffolded (not yet verified locally).

**Changes to handoff.md:**

- Complete rewrite: flat "Snapshot of work completed" structure replaced with a
  persistent **Phase history** heading (Phase 0 condensed to a bullet list) plus a
  **Current status** section for Phase 1. This structure was kept for all subsequent sessions.
- Active branch updated to `feature/phase-1`.
- Phase 1 Done list records the first five tests in `Test1.cs` with their exact method
  names and the behaviour each one pins.
- Pending list: local verification, expansion to 15–25 tests, CI wiring, snapshots.
- Recommended next steps: run existing tests locally, expand coverage, add CI step.
- Quick restart prompt updated: points to `feature/phase-1`; instructs to run
  `dotnet test` first, then expand.

**Complications / deviations:** None.

---

## Session 1a — 2026-03-08 · `4735631` · branch `feature/phase-0`

**Trigger:** Phase 0 complete; CI green on both runners.

**References:** CI run `22820942946` (both `windows-2022` and `windows-2025` green) · PR [#1](https://github.com/CabaVarga/WordNetAPI/pull/1) pending at time of writing · 0 tests (build-only CI).

**First version of handoff.md.** No prior state to diff against.

**Contents:**

- Context block: repo path, active branch (`feature/phase-0`), remote URLs, `gh` auth.
- "Snapshot of work completed" — a flat narrative covering the full Phase 0 arc:
  - Initial docs suite added (`modernization-plan.md`, `lair-dependencies.md`, `fork-plan.md`).
  - CI workflow added and iterated through three failing configurations before settling on
    the `windows-2022` (required) / `windows-2025` (canary) matrix.
  - `Directory.Build.props` + `FrameworkPathOverride` workaround for `net40` on hosted runners.
  - `global.json` SDK pin (`9.0.300`).
  - `AllRules.ruleset` dangling reference warnings removed from both legacy csproj files.
  - `WordNet.csproj` Release `OutputPath` fixed from broken `Libraries\` path to `bin\Release\`.
  - `README.md` expanded with prerequisites, build commands, data layout, CI badge.
- "Current status (Phase 0)" checklist — all items ticked.
- Recommended next step: start Phase 1 (test fixture, 5 tests, CI wiring).
- Quick restart prompt: points to `feature/phase-0`; CI green; directs to Phase 1.

**Complications / deviations:**

- CI required three iterations to stabilise: `windows-2019` runner label does not exist
  on GitHub-hosted runners (queued jobs blocked until cancelled); Chocolatey-based
  targeting-pack install failed; final fix used `Directory.Build.props` +
  `FrameworkPathOverride` driven by a `nuget.exe`-installed reference assembly package.
