# WordNetAPI Under-the-Hood Flow

Date: 2026-03-07  
Companion to: `docs/under-the-hood.md`

## System Overview

```mermaid
flowchart LR
    A["WordNet files in resources/<br/>index.* + data.*"] --> B["WordNetEngine ctor"]
    B --> C{"inMemory?"}
    C -->|true| D["Preload all synsets and word maps"]
    C -->|false| E["Open binary index search streams + data readers"]
    D --> F["API methods: GetSynSet/GetSynSets/GetMostCommonSynSet/AllWords"]
    E --> F
    F --> G["SynSet graph operations<br/>(relations, paths, depth, lexical links)"]
```

## Engine Initialization Flow

```mermaid
flowchart TD
    A["new WordNetEngine(wordNetDirectory, inMemory)"] --> B["Validate directory exists"]
    B --> C["Build required file list:<br/>data.adj/adv/noun/verb + index.adj/adv/noun/verb"]
    C --> D["Verify all files exist"]
    D --> E{"'.sorted_for_dot_net' exists?"}
    E -->|No| F["Resort all index.* by .NET string order"]
    F --> G["Write .sorted_for_dot_net marker"]
    E -->|Yes| H["Keep existing sorted index files"]
    G --> I{"inMemory?"}
    H --> I
    I -->|true| J["In-memory init (3 passes over data.*)<br/>+ build pos->word->synsets map from index.*"]
    I -->|false| K["Disk init:<br/>BinarySearchTextStream per index.*<br/>StreamReader per data.*"]
    J --> L["Ready"]
    K --> L
```

## In-Memory Mapping (Detailed)

```mermaid
flowchart TD
    A["Pass 1: scan data.*"] --> B["Count synset lines (non-header)"]
    B --> C["Pass 2: create SynSet shells<br/>(POS + offset only) into _idSynset"]
    C --> D["Pass 3: instantiate every SynSet<br/>from data lines + wire relations"]
    D --> E["Scan index.* lines"]
    E --> F["Build _posWordSynSets:<br/>POS -> word -> Set<SynSet>"]
    F --> G["Mark most-common synset for ambiguous word/POS"]
    G --> H["Lookups become dictionary-based"]
```

## Disk-Based Mapping (Detailed)

```mermaid
flowchart TD
    A["Initialize per-POS BinarySearchTextStream for index.*"] --> B["Initialize per-POS StreamReader for data.*"]
    B --> C["On query: normalize word (lowercase + spaces->_)"]
    C --> D["Binary search index line for that POS"]
    D --> E{"index line found?"}
    E -->|No| F["Return empty set / null"]
    E -->|Yes| G["Parse final offset tokens into SynSet shells"]
    G --> H["For each shell: seek data.* to offset + parse SynSet"]
    H --> I["Return instantiated SynSet objects"]
```

## `GetSynSets` Call Flow

```mermaid
sequenceDiagram
    participant Caller
    participant Engine as WordNetEngine
    participant Index as index.* access
    participant Data as data.* access
    participant Syn as SynSet

    Caller->>Engine: GetSynSets(word, pos[])
    Engine->>Engine: normalize word (ToLower + Replace(' ', '_'))
    alt in-memory mode
        Engine->>Engine: read _posWordSynSets[pos][word]
        Engine-->>Caller: Set<SynSet>
    else disk mode
        Engine->>Index: binary search for word line
        Index-->>Engine: index line (or null)
        alt found
            Engine->>Engine: parse synset offsets from line
            loop each offset
                Engine->>Syn: new SynSet(pos, offset, engine)
                Engine->>Data: seek + read definition by offset
                Engine->>Syn: Instantiate(definition, null)
            end
            Engine-->>Caller: Set<SynSet>
        else not found
            Engine-->>Caller: empty set
        end
    end
```

## `GetMostCommonSynSet` Flow

```mermaid
flowchart TD
    A["GetMostCommonSynSet(word, pos)"] --> B["Normalize word"]
    B --> C["GetSynSets(word, pos)"]
    C --> D{"SynSet count"}
    D -->|0| E["return null"]
    D -->|1| F["return that one"]
    D -->|>1| G["find synset flagged IsMostCommonSynsetFor(word)"]
    G --> H{"found exactly one?"}
    H -->|Yes| I["return it"]
    H -->|No| J["throw (legacy invariant violation)"]
```

## `GetSynSet` by ID Flow

```mermaid
flowchart TD
    A["GetSynSet('POS:Offset')"] --> B{"inMemory?"}
    B -->|true| C["Dictionary lookup in _idSynset"]
    B -->|false| D["Parse POS + offset from ID"]
    D --> E["Create SynSet shell"]
    E --> F["Read definition from data.* at byte offset"]
    F --> G["Instantiate shell"]
    C --> H["Return SynSet"]
    G --> H
```

## SynSet Internal Parsing Flow

```mermaid
flowchart TD
    A["SynSet.Instantiate(definition, idSynset?)"] --> B["Parse lexicographer file number"]
    B --> C["Parse hex word count + words"]
    C --> D["Parse gloss after '|'"]
    D --> E["Parse relation count"]
    E --> F["For each relation tuple (symbol, offset, pos, src/tgt indexes)"]
    F --> G{"src=tgt=0?"}
    G -->|Yes| H["Add semantic relation edge"]
    G -->|No| I["Add lexical relation mapping"]
    H --> J["Mark instantiated"]
    I --> J
```

## Practical Interpretation

- In-memory mode front-loads parsing cost for fastest repeated queries.
- Disk mode minimizes memory but does more file IO per query.
- Offsets in index lines are the bridge from lexical lookup to full synset records.
- Relation symbols are interpreted per POS, not globally.

---

## Debug Cookbook

Use this section when behavior looks wrong in production, tests, or local exploration.

## 1) "No synsets found" for a word you expect to exist

### Likely causes

- Input normalization mismatch (spaces/case are normalized, morphology is not).
- Querying wrong POS.
- Index files are not sorted for .NET but binary search is being used.
- Dataset version differs from what your expectation/examples were based on.

### Verify quickly

1. Confirm normalized query behavior:
   - compare `"new york"` vs `"New York"` (should match)
   - compare inflected form like `"running"` vs base form `"run"` (may differ)
2. Query all POS (`GetSynSets(word)` with no restriction) to rule out POS mismatch.
3. Check presence of `.sorted_for_dot_net` in the data directory.
4. Confirm the word exists in the relevant `index.*` file.

### Inspect code

- `src/WordNet/WordNetEngine.cs` (`GetSynSets`)
- `src/WordNet/WordNetEngine.cs` (constructor sorting block)

## 2) `GetMostCommonSynSet` returns `null`

### Likely causes

- No synsets exist for the normalized word+POS.
- Data mismatch or query POS is wrong.

### Verify quickly

1. Call `GetSynSets(word, pos)` first and inspect count.
2. If count is `0`, `null` is expected.
3. If count is `1`, `GetMostCommonSynSet` should return that one.
4. If count is `>1` and still failing, treat as invariant/flagging issue.

### Inspect code

- `src/WordNet/WordNetEngine.cs` (`GetMostCommonSynSet`)
- `src/WordNet/WordNetEngine.cs` (`GetSynSetShells` + flag assignment in `GetSynSets`)
- `src/WordNet/SynSet.cs` (`SetAsMostCommonSynsetFor`, `IsMostCommonSynsetFor`)

## 3) Exception: "Multiple most common synsets found"

### Likely causes

- Most-common flag set on multiple synsets for same word+POS.
- Duplicate or corrupted index ordering assumptions.

### Verify quickly

1. Rebuild the index source assumption by checking raw index line for that word.
2. Confirm offset order and no duplicate offset tokens.
3. Repeat with a fresh dataset copy.

### Inspect code

- `src/WordNet/WordNetEngine.cs` (`GetSynSetShells`, `GetMostCommonSynSet`)
- `src/WordNet/SynSet.cs` (flag storage)

## 4) Exception during constructor: missing required WordNet files

### Likely causes

- Wrong data directory.
- Partial copy of dataset.

### Verify quickly

1. Confirm these files exist in target directory:
   - `data.adj`, `data.adv`, `data.noun`, `data.verb`
   - `index.adj`, `index.adv`, `index.noun`, `index.verb`
2. Ensure app/test config points to the intended path.

### Inspect code

- `src/WordNet/WordNetEngine.cs` (constructor required-file validation)

## 5) Constructor mutates files unexpectedly

### Symptom

- First run rewrites `index.*` files and creates `.sorted_for_dot_net`.

### Why it happens

- Legacy compatibility behavior to make binary search align with .NET sort order.

### Verify quickly

1. Remove `.sorted_for_dot_net` and run once in a disposable copy.
2. Observe rewritten `index.*` and marker creation.

### Inspect code

- `src/WordNet/WordNetEngine.cs` (constructor sorting region)

## 6) `ArgumentOutOfRangeException` from substring logic in disk mode

### Symptom

- Exception around parsing current index line word in binary search callback.

### Likely causes

- Unexpected line format during search (blank/malformed line).
- Concurrency against shared stream state in disk mode tests or multithreaded use.

### Verify quickly

1. Re-run with single-threaded access.
2. Disable test parallelization if sharing one engine instance.
3. Check whether line has a space delimiter before substring assumptions.

### Inspect code

- `src/WordNet/WordNetEngine.cs` (binary search delegate in disk-mode initialization)
- `src/WordNet.Tests/MSTestSettings.cs` (if test-only symptom)

## 7) "Position mismatch" when instantiating synset by offset

### Symptom

- Exception from `GetSynSetDefinition` validating offset vs read line.

### Likely causes

- Invalid/foreign offset from index line.
- Data/index file mismatch (different WordNet version set mixed together).
- Stream position interference under concurrent disk-mode access.

### Verify quickly

1. Confirm all `data.*` and `index.*` come from same dataset snapshot.
2. Reproduce with a single-threaded, fresh engine instance.
3. Manually inspect the target `data.*` line around the offset.

### Inspect code

- `src/WordNet/WordNetEngine.cs` (`GetSynSetDefinition`)
- `src/WordNet/SynSet.cs` (`Instantiate`)

## 8) Relation parsing fails with "Unexpected POS" or missing symbol map key

### Likely causes

- Dataset contains relation symbol/POS pattern not covered by static map.
- Corrupt data line.

### Verify quickly

1. Capture the exact failing definition line.
2. Identify relation tuple symbol and related POS code.
3. Compare against static `_posSymbolRelation` mapping.

### Inspect code

- `src/WordNet/WordNetEngine.cs` (static symbol map + `GetSynSetRelation`)
- `src/WordNet/SynSet.cs` (`GetPOS`, relation loop in `Instantiate`)

## 9) Memory pressure or long startup in in-memory mode

### Likely causes

- Full preload of all synsets and relation graph is expensive.

### Verify quickly

1. Compare startup time/memory between `inMemory=true` and `inMemory=false`.
2. Profile object count for `SynSet` and relation containers.

### Inspect code

- `src/WordNet/WordNetEngine.cs` (in-memory three-pass initialization)

## 10) Slow query latency in disk mode

### Likely causes

- Per-query index search + per-synset file seek/parse.
- High relation traversal causing repeated instantiation.

### Verify quickly

1. Benchmark same query in both modes.
2. Measure cost of first call vs repeated call patterns.
3. Check if callers repeatedly query same word set (cache opportunity).

### Inspect code

- `src/WordNet/WordNetEngine.cs` (`GetSynSets`, `GetSynSetDefinition`)
- `src/WordNet/SynSet.cs` (lazy relation traversal calls)

## 11) Test flakiness in modern test runners

### Likely causes

- Legacy engine in disk mode is not safe for parallel access with shared instance.

### Verify quickly

1. Run tests with assembly-level no-parallelization.
2. Use one engine instance per test if parallelization is required.

### Inspect code

- `src/WordNet.Tests/MSTestSettings.cs`
- `src/WordNet.Tests/Test1.cs`

## 12) Quick triage checklist

When a new issue appears, run this order:

1. Confirm data directory and required 8 files.
2. Confirm `.sorted_for_dot_net` status.
3. Reproduce in single-threaded mode.
4. Compare behavior in `inMemory=false` vs `inMemory=true`.
5. Capture failing word/POS/synset ID and one raw source line from `index.*` or `data.*`.
6. Map symptom to method path in `WordNetEngine` or `SynSet`.
