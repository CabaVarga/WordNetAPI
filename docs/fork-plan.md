# Fork and Migration Plan

Date: 2026-03-08

## Goal

Move ongoing modernization work from the current clone (tracking upstream) to your own fork, then continue development and CI from the fork.

## Current repo state (detected)

- Current local branch: `feature/phase-0`
- Current remote `origin`: `https://github.com/CabaVarga/WordNetAPI.git`
- Current remote `upstream`: `https://github.com/zacg/WordNetAPI.git`

## Recommended approach

Use this existing local clone as a transfer source, push your branch to your fork, then optionally start a fresh local clone from your fork for day-to-day work.

## Step 1 - Create the GitHub fork

1. Open the upstream repository in GitHub:
   - [https://github.com/zacg/WordNetAPI](https://github.com/zacg/WordNetAPI)
2. Click `Fork`.
3. Create it under your account/org.

Fork URL:

- `https://github.com/CabaVarga/WordNetAPI.git`

## Step 2 - Preserve current local work

From `D:\wordnet\WordNetAPI`:

```powershell
git status
```

If you have uncommitted changes, choose one:

- Preferred: commit them on `feature/phase-0`
- Alternative: stash temporarily

```powershell
# Option A: commit
git add -A
git commit -m "chore: checkpoint before fork migration"

# Option B: stash
git stash push -u -m "fork-migration-temp"
```

## Step 3 - Rewire remotes (fork workflow)

Once your fork exists, run:

```powershell
# Keep upstream reference to original repo
git remote rename origin upstream

# Add your fork as origin
git remote add origin https://github.com/CabaVarga/WordNetAPI.git

# Verify
git remote -v
```

Expected:

- `origin` -> your fork
- `upstream` -> `zacg/WordNetAPI`

## Step 4 - Push your branch and history to the fork

```powershell
git push -u origin feature/phase-0
```

If you used stash in Step 2, restore it now:

```powershell
git stash list
git stash pop
```

Then commit and push those changes:

```powershell
git add -A
git commit -m "chore: restore local changes after fork migration"
git push
```

## Step 5 - Fresh clone from fork (recommended for this migration)

If you want to stop using this original clone entirely:

```powershell
cd D:\
git clone https://github.com/CabaVarga/WordNetAPI.git WordNetAPI-fork
cd .\WordNetAPI-fork
git checkout feature/phase-0
git remote add upstream https://github.com/zacg/WordNetAPI.git
git remote -v
```

From here forward, work in `D:\WordNetAPI-fork`.

## Context and conversation transfer

> **This section describes the state at the time the fork migration was performed (2026-03-08, Phase 0).**
> For the current working context and restart prompt, always refer to `docs/handoff.md`.

- Working folder: `D:\WordNetAPI-fork`
- Fork remote: `origin` -> `https://github.com/CabaVarga/WordNetAPI.git`
- Upstream remote: `upstream` -> `https://github.com/zacg/WordNetAPI.git`
- Core planning docs:
  - `docs/handoff.md` ← start here for current state
  - `docs/modernization-plan.md`
  - `docs/lair-dependencies.md`
  - `docs/fork-plan.md`

## Step 6 - Run CI from the fork

After pushing `feature/phase-0` to your fork:

1. Open your fork on GitHub.
2. Confirm `Actions` is enabled.
3. Verify `CI Build` workflow runs on branch push.
4. If needed, trigger manually via `workflow_dispatch`.

## Step 7 - Keep fork synced with upstream

Periodically pull upstream changes:

```powershell
git fetch upstream
git checkout master
git merge upstream/master
git push origin master
```

Then rebase or merge your feature branch as needed.

## Notes specific to current modernization work

- `docs/modernization-plan.md` now includes a stop-point snapshot and next-step plan.
- `.github/workflows/ci-build.yml` is already in this branch, so pushing branch to your fork should trigger CI immediately.

## Recommended now (copy/paste sequence)

Use this exact sequence from the current repo (`D:\wordnet\WordNetAPI`) to transfer work to your fork safely:

```powershell
git status
git add -A
git commit -m "chore: checkpoint before fork migration"
git remote rename origin upstream
git remote add origin https://github.com/CabaVarga/WordNetAPI.git
git push -u origin feature/phase-0
cd D:\
git clone https://github.com/CabaVarga/WordNetAPI.git WordNetAPI-fork
cd .\WordNetAPI-fork
git checkout feature/phase-0
git remote add upstream https://github.com/zacg/WordNetAPI.git
git remote -v
```
