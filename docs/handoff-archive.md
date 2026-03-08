# WordNetAPI Handoff Archive

Chronological record of each `docs/handoff.md` revision, newest first.
Each entry summarises what was new or changed relative to the prior version.

---

## Session 4 — 2026-03-08 · `c215899` · branch `feature/phase-3`

**Trigger:** PR #3 merged; `master` pulled; `feature/phase-3` created.

**Changes to handoff.md:**

- Active branch updated to `feature/phase-3`.
- Phase 2 moved into phase history section and condensed to a single bullet block, now
  including the `.gitattributes` fix (binary encoding for `resources/data.*` / `index.*`)
  that had been added as a hotfix commit during the PR.
- "Current status" section replaced entirely: new Phase 3 goal documented — stabilise
  `LAIR.*` dependency story using **Option A** (inline/replace) from `docs/lair-dependencies.md`.
- Staged plan table added: A1 (`LAIR.Extensions`), A2 (`BinarySearchTextStream`), A3.1
  (`Set<T>` shim), with risk ratings.
- Pending checklist reflects the three extraction steps plus downstream project clean-up.
- Recommended next steps updated to A1 first (lowest risk, mechanical substitutions).
- Quick restart prompt updated to `feature/phase-3` and points to A1 as the entry point.

---

## Session 3 — 2026-03-08 · `6e1d77e` · branch `feature/phase-2`

**Trigger:** Phase 2 implementation complete; PR #3 not yet opened.

**Changes to handoff.md:**

- Date line updated to "session 3".
- "Current status" header changed from "Phase 2" to "Phase 2 — complete, pending PR".
- Done list condensed: carry-over detail removed; Phase 2 implementation items added:
  - `SortIndexFiles()` extraction and `InvalidOperationException` guard in constructor.
  - `PreprocessingTests.cs` with two new tests.
  - 28/28 test count.
- Pending item changed from task list to a single note: "PR not yet created (do not push until instructed)."
- Recommended next steps replaced: PR #3, then Phase 3 strategy decision, then Phase 3A start.
- Quick restart prompt updated: branch still `feature/phase-2`; note that Phase 2 is complete
  but not yet pushed; summarises `SortIndexFiles` / guard / new tests; says next step is PR #3.

---

## Session 2b — 2026-03-08 · `bd38500` · branch `feature/phase-2`

**Trigger:** All Phase 1 carry-overs committed; working tree confirmed clean; branch pushed.

**Changes to handoff.md:**

- Date line updated to "session 2".
- Done list expanded: carry-over detail replaced with full list of committed files
  (`ci-build.yml`, `Test1.cs`, `TestHelpers.cs`, three new test files, docs).
- 26/26 tests confirmed passing (Release config) added to Done.
- "Branch pushed to `origin/feature/phase-2`" added to Done.
- Carry-over commit item removed from Pending.
- Pending list now contains only the Phase 2 technical tasks (audit, extract, guard, tests).
- Recommended next steps simplified: carry-over step removed; starts directly from "Audit the constructor."
- Quick restart prompt updated: carry-over instruction dropped; states "working tree is clean,
  all carry-overs committed, 26/26 tests pass, branch pushed"; directs to start at constructor audit.

---

## Session 2a — 2026-03-08 · `682abc7` · branch `feature/phase-2`

**Trigger:** PR #2 merged; `master` pulled; `feature/phase-2` created; carry-over situation discovered.

**Changes to handoff.md:**

- Active branch updated to `feature/phase-2`.
- Phase 1 promoted from "Current status" into "Phase history" (condensed bullet block).
- "Current status" section reset to Phase 2 scope.
- Phase 2 goal documented for the first time: remove implicit index-file mutation.
- Done list is minimal: branch created; two carry-over files staged but NOT yet committed
  (rescued from a dropped stash — the Phase 1 dotnet-test CI step and `Test1.cs`
  `TestHelpers` delegation were never included in PR #2).
- Pending list leads with a prominent "Commit the staged carry-over files" instruction
  before any Phase 2 technical tasks.
- Recommended next steps: step 1 is explicit `git status` / `git add` / `git commit` /
  `dotnet test` sequence for the carry-overs; steps 2–5 cover Phase 2 design.
- Quick restart prompt carries the same carry-over warning as the most urgent first action.

---

## Session 1b — 2026-03-08 · `296f3fb` · branch `feature/phase-1`

**Trigger:** Phase 0 merged via PR #1; `feature/phase-1` created; initial test suite scaffolded.

**Changes to handoff.md:**

- Complete rewrite: flat "Snapshot of work completed" structure replaced with a
  persistent **Phase history** heading (Phase 0 condensed to bullet list) plus a
  **Current status** section for Phase 1.
- Active branch updated to `feature/phase-1`.
- Phase 1 Done list records first five tests in `Test1.cs` with their exact method names
  and what behaviour each pins.
- Pending list: local verification, expansion to 15–25 tests, CI wiring, canonical snapshots.
- Recommended next steps: run existing tests locally, expand coverage, add CI step.
- Quick restart prompt updated: points to `feature/phase-1`; instructs to run
  `dotnet test` first to confirm 5 tests pass, then expand.

---

## Session 1a — 2026-03-08 · `4735631` · branch `feature/phase-0`

**Trigger:** Phase 0 complete; CI green on both runners (run `22820942946`).

**First version of handoff.md.** No prior state to diff against.

**Contents:**

- Context block: repo path, active branch (`feature/phase-0`), remote URLs, `gh` auth.
- "Snapshot of work completed" — a flat narrative covering the full Phase 0 arc:
  - Initial docs suite added (`modernization-plan.md`, `lair-dependencies.md`,
    `fork-plan.md`).
  - CI workflow added and iterated through three failing configurations
    (`windows-2019` attempt, Chocolatey install attempt, `windows-2022`/`2025` matrix
    final form).
  - `Directory.Build.props` + `FrameworkPathOverride` workaround for `net40` on hosted runners.
  - `global.json` SDK pin (`9.0.300`).
  - `AllRules.ruleset` dangling reference warnings removed from both legacy csproj files.
  - `WordNet.csproj` Release `OutputPath` fixed from broken `Libraries\` path to `bin\Release\`.
  - `README.md` expanded with prerequisites, build commands, data layout, CI badge.
  - CI run `22820942946`: both runners green.
- "Current status (Phase 0)" checklist — all items ticked.
- Recommended next step: start Phase 1 (test fixture, 5 deterministic tests, CI wiring).
- Quick restart prompt: points to `feature/phase-0`; says Phase 0 is complete and CI is
  green; directs to start Phase 1 characterization tests.
