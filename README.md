WordNetAPI
==========

A C# API for the [WordNet](https://wordnet.princeton.edu/) lexical database, originally written by Matt Gerber at Michigan State University.

Project homepage: https://ptl.sys.virginia.edu/ptl/members/matthew-gerber/software#wordnet

---

## Prerequisites

| Requirement | Version |
|---|---|
| .NET SDK | 9.0 (pinned via `global.json`) |
| .NET Framework targeting pack | 4.0 (installed with Visual Studio 2010–2019, or via [Dev Pack](https://aka.ms/msbuild/developerpacks)) |
| WordNet data files | 3.1 (place in `resources/` — see below) |

The `LAIR.Collections`, `LAIR.Extensions`, and `LAIR.IO` assemblies are vendored in `lib/` and do not require a separate install.

---

## Build

```powershell
# Restore NuGet packages
dotnet restore src/WordNet.sln

# Build (Debug)
dotnet build src/WordNet.sln

# Build (Release)
dotnet build src/WordNet.sln --configuration Release
```

The Release output for the library lands in `src/WordNet/bin/Release/`.

---

## WordNet data files

The engine reads the Princeton WordNet 3.1 data files from a `resources/` directory.
Place the standard WordNet distribution contents there before running tests or the sample application:

```
resources/
  data.adj
  data.adv
  data.noun
  data.verb
  index.adj
  index.adv
  index.noun
  index.verb
  ...
```

The data files are **not** included in this repository. Download them from
<https://wordnet.princeton.edu/download/current-version>.

> **Note:** The engine no longer rewrites `index.*` files during normal runtime.
> Before first use, run the explicit preprocessing step once to create the
> `.sorted_for_dot_net` marker:
>
> ```csharp
> WordNetEngine.SortIndexFiles(wordNetDirectory);
> ```
>
> If the marker is missing, `WordNetEngine` throws `InvalidOperationException`.

---

## Projects

| Project | Type | Description |
|---|---|---|
| `src/WordNet` | Library (net40) | Core WordNet API |
| `src/TestApplication` | WinForms app (net40) | Interactive test harness |
| `src/WordNet.Tests` | Test project (net48) | MSTest characterization tests |

---

## Thread safety and lifetime

- `WordNetEngine` now implements `IDisposable`. `Close()` is retained as a
  compatibility shim and calls `Dispose()`.
- After disposal, API methods throw `ObjectDisposedException`.
- Read operations are synchronized internally in disk mode; concurrent reads are
  supported for typical usage.
- Do not call `Dispose()` concurrently with active API calls.

## CI

[![CI Build](https://github.com/CabaVarga/WordNetAPI/actions/workflows/ci-build.yml/badge.svg)](https://github.com/CabaVarga/WordNetAPI/actions/workflows/ci-build.yml)

The CI workflow runs on `windows-2022` (required) and `windows-2025` (canary, non-blocking).
It installs the .NET Framework 4.0 reference assemblies via NuGet to work around the
absence of the net40 targeting pack on GitHub-hosted runners.

---

## Modernization roadmap

See [`docs/modernization-plan.md`](docs/modernization-plan.md) for the phased plan to
modernize the build system, remove LAIR dependencies, add characterization tests, and
migrate to SDK-style projects.
