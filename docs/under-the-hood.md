# WordNetAPI Under the Hood

Date: 2026-03-07

This document explains:

1. How WordNet source data is structured.
2. How this .NET codebase maps source data into runtime objects.
3. How `WordNetEngine` exposes that mapped data.

---

## 1) How WordNet Data Is Structured

## Files in `resources/`

The engine directly relies on eight core files:

- `index.adj`
- `index.adv`
- `index.noun`
- `index.verb`
- `data.adj`
- `data.adv`
- `data.noun`
- `data.verb`

Everything else in `resources/` (for example `*.exc`, `dbfiles/*`, `cntlist`, `sents.vrb`) is not required by `WordNetEngine` in this repository.

The constructor validates that all 8 required files exist.

## POS split

WordNet is physically partitioned by part of speech:

- `adj` -> `Adjective`
- `adv` -> `Adverb`
- `noun` -> `Noun`
- `verb` -> `Verb`

That mapping is encoded in `WordNetEngine.GetFilePOS()`.

## Header lines vs data lines

In both `index.*` and `data.*`, the first block of lines is a text/license header.
In this implementation, header lines are identified by a leading space (`line.StartsWith(" ")`) and skipped for lexical/synset parsing.

## `index.*` line shape

A typical index line looks like:

`dog n 7 5 ! @ ~ %p #m 7 1 02084071 10114209 ...`

The engine does not parse every field. The critical pieces it uses are:

- **word**: first token (e.g., `dog`)
- **synset count**: token at position `2` (`parts[2]`)
- **synset offsets**: last `synset count` tokens in the line

`GetSynSetShells()` uses this logic:

- split line by whitespace
- read `numSynSets = int.Parse(parts[2])`
- read last `numSynSets` tokens as integer offsets
- build `SynSet` shells as `POS + offset`
- mark the first-sense (most common) synset as the earliest offset in index sense order

## `data.*` line shape

A data line contains:

- byte offset (line starts with it)
- lexical file number
- POS
- word count (hex)
- word/lex_id pairs
- pointer count
- pointer tuples (relation symbol, target offset, target POS, source/target word indexes)
- gloss text after `|`

Example beginning from `data.noun`:

`00001740 03 n 01 entity 0 003 ~ 00001930 n 0000 ... | that which is perceived ...`

`SynSet.Instantiate()` parses this manually with field scanning:

- lexical file number via `GetField(definition, 1)` then `+1` to map into enum
- word count via `GetField(..., 3)` interpreted as hex
- words by walking token positions
- gloss as substring after `|`
- relation count after the word block
- each relation as 4 fields:
  - relation symbol (e.g., `@`, `~`, `%p`, `+`)
  - related synset offset
  - related synset POS code (`n`, `v`, `a`/`s`, `r`)
  - source/target word indexes encoded as 2-byte hex halves (e.g., `0000`)

If source and target indexes are both `0`, the relation is treated as a semantic synset-level edge. Otherwise it is stored as a lexical word-to-word relation.

## Relation symbols are POS-dependent

The same symbol set is not globally valid across all POS.  
`WordNetEngine` builds a static per-POS symbol map at startup (`_posSymbolRelation`) and resolves symbols through `GetSynSetRelation(pos, symbol)`.

---

## 2) How the .NET App Maps Source Data

There are two storage strategies:

- **in-memory mode** (`inMemory = true`)
- **disk mode** (`inMemory = false`)

Both use the same external file format and public API shape; they differ in when/how data is materialized.

## Shared domain model: `SynSet`

A `SynSet` is keyed by immutable identity:

- `POS`
- `Offset`
- `ID` = `POS:Offset` (example: `Noun:2086723`)

The class stores:

- words (`Words`)
- gloss (`Gloss`)
- semantic relation adjacency (`relation -> set of synsets`)
- lexical relation adjacency (`relation -> related synset -> source word index -> target word indexes`)

It also supports graph operations such as:

- `GetRelatedSynSets(...)`
- `GetShortestPathTo(...)`
- `GetClosestMutuallyReachableSynset(...)`
- `GetDepth(...)`
- `GetLexicallyRelatedWords()`

## Runtime index sorting side effect

On startup, `WordNetEngine` checks for `.sorted_for_dot_net` in the data directory.
If absent, it rewrites all `index.*` files sorted by .NET string ordering and then writes `.sorted_for_dot_net`.

Why this exists:

- Binary search in disk mode assumes .NET lexical ordering.
- Original WordNet distribution sort order can differ from .NET.

Operational consequence:

- First run mutates the dataset on disk.

## In-memory mode mapping (`inMemory = true`)

Initialization is a 3-pass preload:

1. **Count synsets** in all `data.*` files (non-header lines).
2. **Create synset shells** (`POS`, `offset`) into `_idSynset`.
3. **Instantiate all synsets** from full `data.*` lines, wiring all references via `_idSynset`.

Then it scans all `index.*` files to build:

- `_posWordSynSets`: `POS -> word -> Set<SynSet>`
- most-common-sense flags per ambiguous word/POS pair

The result is fast lookup at higher memory cost.

## Disk mode mapping (`inMemory = false`)

Initialization opens:

- one `BinarySearchTextStream` per `index.*` in `_posIndexWordSearchStream`
- one `StreamReader` per `data.*` in `_posSynSetDataFile`

Lookup flow:

1. normalize query word (`ToLower()`, spaces -> underscores)
2. binary-search index file for matching line
3. parse offsets from index line into synset shells
4. lazily instantiate each synset by seeking to offset in `data.*`

This minimizes memory but makes each lookup IO-heavy and more sensitive to stream concurrency.

## Most-common sense mapping

Most-common sense is inferred from index ordering:

- `GetSynSetShells()` marks one synset as candidate "most common"
- if more than one synset exists for word+POS, that synset gets a flag (`SetAsMostCommonSynsetFor`)

Note: XML docs on `GetMostCommonSynSet()` say this is only for memory mode, but current implementation also supports it in disk mode by setting the same flags during lookup.

---

## 3) How `WordNetEngine` Exposes Data

## Constructor

`WordNetEngine(string wordNetDirectory, bool inMemory)`

Responsibilities:

- validate directory and required files
- optionally rewrite index files for .NET sort order
- initialize either in-memory maps or disk-based search streams/readers

## Core properties

- `InMemory`: true/false mode flag
- `WordNetDirectory`: dataset root path
- `AllWords`: returns `Dictionary<POS, Set<string>>`
  - in memory mode: from `_posWordSynSets` keys
  - in disk mode: by scanning index files and extracting first token per non-header line

## Core retrieval API

- `GetSynSet(string synsetID)`
  - In-memory: direct dictionary lookup by `POS:Offset`
  - Disk: parse ID, create shell, instantiate from `data.*` offset

- `GetSynSets(string word, params POS[] posRestriction)`
  - Defaults to all POS if none passed
  - Normalizes input (`lowercase`, spaces->underscores)
  - Returns all matching synsets across requested POS
  - In-memory: dictionary lookup by normalized word
  - Disk: binary search index -> instantiate returned shells

- `GetMostCommonSynSet(string word, POS pos)`
  - Uses same normalization as `GetSynSets`
  - If exactly one synset, returns it
  - If multiple, returns the one flagged as most common
  - Returns `null` if no synsets found

## Internal retrieval helper

- `GetSynSetDefinition(POS pos, int offset)` (internal)
  - seeks stream to byte offset
  - reads line
  - validates line starts with same offset

This method is the disk-mode bridge from index offsets to full synset records.

## Lifecycle

- `Close()` releases mode-specific resources:
  - memory mode: nulls large dictionaries
  - disk mode: closes binary-search streams and data readers

The class does **not** implement `IDisposable`; callers must remember to call `Close()`.

---

## End-to-End Flow (Mental Model)

For `GetSynSets("new york", POS.Noun)`:

1. Normalize -> `new_york`.
2. Locate index line in `index.noun`.
3. Parse final offsets in that line.
4. For each offset:
   - locate line at same offset in `data.noun`
   - parse words, gloss, and relation pointers
   - create/link `SynSet` objects
5. Return a set of `SynSet` instances with traversable relations.

---

## Practical Notes for This Repository

- Project docs mention WordNet 3.0, but checked-in resource headers indicate WordNet 3.1.
- Engine behavior depends on .NET lexical sorting of `index.*`; hence `.sorted_for_dot_net`.
- Morphological normalization (for plurals, tense variants, etc.) is not implemented here; only lowercase + underscore normalization is applied.
- Disk mode is memory efficient but not designed as a high-concurrency reader over shared mutable stream state.
