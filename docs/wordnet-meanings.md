# WordNet File Meanings (`resources/`)

Date: 2026-03-07

This document explains what the files in `resources/` mean in WordNet terms, and whether this repository's `WordNetEngine` uses them.

## TL;DR

- **Core runtime files** for WordNet lookup are `index.*` + `data.*` (for `adj`, `adv`, `noun`, `verb`).
- **Morphology helper files** are `*.exc`.
- **Sense-frequency / ranking files** are `cntlist` and `cntlist.rev`.
- **Verb example sentence files** are `sentidx.vrb` and `sents.vrb`.
- **`dbfiles/`** contains lexicographer source files (the authoring inputs to `grind`), not the compiled runtime database used by this .NET engine.

---

## 1) Core compiled database files

These are the canonical WordNet database files documented by `wndb(5WN)`:

- `index.adj`, `index.adv`, `index.noun`, `index.verb`
- `data.adj`, `data.adv`, `data.noun`, `data.verb`

### Meaning

- `index.pos`: alphabetized lemma list for a part of speech, each entry containing byte offsets into `data.pos`.
- `data.pos`: synset records and pointer relations; each line is one synset definition.

### Used by this repo?

**Yes, directly.** `WordNetEngine` requires all 8 files at startup and uses:

- `index.*` for word-to-offset lookup
- `data.*` for offset-to-synset parsing

---

## 2) Morphology exception files

Files:

- `noun.exc`
- `verb.exc`
- `adj.exc`
- `adv.exc`

### Meaning

Irregular inflection mappings used by WordNet morphology (`morphy` behavior), e.g. irregular plural/verb forms to base forms.

### Used by this repo?

**No (currently).** This `WordNetEngine` only normalizes by lowercasing and converting spaces to underscores; it does not implement WordNet morphology rules.

---

## 3) Sense frequency / ordering files

Files:

- `cntlist`
- `cntlist.rev`

### Meaning

- `cntlist`: tagged-sense counts, sorted by frequency; historically used by `grind` to order senses (sense 1 = most frequent estimate).
- `cntlist.rev`: same information keyed for runtime display by sense key.

### Used by this repo?

- **Not read at runtime** by `WordNetEngine`.
- **Conceptually relevant** because `index.*` sense order was originally influenced by this data during WordNet build generation.

---

## 4) Verb sentence files

Files:

- `sentidx.vrb`
- `sents.vrb`
- `verb.Framestext`

### Meaning

- `sentidx.vrb`: map from verb sense key to example sentence template IDs.
- `sents.vrb`: template text, with `%s` placeholder for the verb.
- `verb.Framestext`: generic verb frame text definitions (companion source text often distributed with WordNet resources).

### Used by this repo?

**Not currently used** by `WordNetEngine` or `SynSet` public API in this project, though the underlying WordNet data model supports verb frames.

---

## 5) Lexicographer source files (`dbfiles/`)

Folder:

- `resources/dbfiles/`

Representative files:

- `adj.all`, `adj.pert`, `adj.ppl`
- `adv.all`
- `noun.*` (e.g., `noun.animal`, `noun.person`, `noun.time`, ...)
- `verb.*` (e.g., `verb.motion`, `verb.communication`, `verb.weather`, ...)
- plus helper files like `cntlist` and `verb.Framestext`

### Meaning

These are **authoring/source inputs** used by WordNet’s `grind` tool to produce compiled DB files (`index.*`, `data.*`, and optionally `index.sense`).

Think of these as "source code" for WordNet lexical content; `index.*`/`data.*` are the "compiled artifacts."

### Used by this repo?

**No.** This code consumes compiled DB files only.

---

## 6) Build/provenance marker files

Files:

- `log.grind.3.1`
- `.sorted_for_dot_net`

### Meaning

- `log.grind.3.1`: build log/provenance output from a WordNet database generation run (`grind`).
- `.sorted_for_dot_net`: marker created by this .NET project indicating it has resorted `index.*` files for .NET binary-search ordering.

### Used by this repo?

- `log.grind.3.1`: not required by engine logic.
- `.sorted_for_dot_net`: **yes** (controls whether startup rewrites index files).

---

## 7) What `WordNetEngine` actually depends on today

Hard requirement on startup:

- `data.adj`, `data.adv`, `data.noun`, `data.verb`
- `index.adj`, `index.adv`, `index.noun`, `index.verb`

Conditional behavior:

- if `.sorted_for_dot_net` is absent, it rewrites `index.*` to .NET sort order and creates the marker.

Everything else in `resources/` is currently auxiliary from this codebase's runtime perspective.

---

## 8) Version note for this repository

Your local files contain headers indicating **WordNet 3.1** licensing text, while many upstream manpages are published under the 3.0 documentation tree. For the files discussed here, formats and semantics are still aligned for practical usage in this project.

---

## References (online)

- [wndb(5WN): WordNet database files](https://wordnet.princeton.edu/documentation/wndb5wn)
- [senseidx(5WN): sense index format](https://wordnet.princeton.edu/node/22)
- [cntlist(5WN): tagged sense counts](https://wordnet.princeton.edu/documentation/cntlist5wn)
- [lexnames(5WN): lexicographer file names](https://wordnet.princeton.edu/node/18)
- [wninput(5WN): lexicographer input file format](https://wordnet.princeton.edu/node/30)
- [grind(1WN): build process from lexicographer files](https://wordnet.princeton.edu/node/17)
