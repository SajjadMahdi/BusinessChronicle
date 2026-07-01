# BusinessChronicle — Architecture Document

> **Git-like Versioning & Auditing for Modern .NET Applications**

| Document | Version | Status |
|----------|---------|--------|
| Architecture | 0.1.0-draft | Design Phase |
| Target .NET | .NET 9+ (LTS-aligned) | — |
| Last Updated | 2026-07-01 | — |

---

## Table of Contents

1. [Vision](#1-vision)
2. [Technology Stack](#2-technology-stack)
3. [Solution Structure](#3-solution-structure)
4. [Dependency Rules](#4-dependency-rules)
5. [Package Strategy](#5-package-strategy)
6. [Public API Philosophy](#6-public-api-philosophy)
7. [Coding Standards](#7-coding-standards)
8. [Performance Strategy](#8-performance-strategy)
9. [Extensibility](#9-extensibility)
10. [UI Strategy](#10-ui-strategy)
11. [Documentation Strategy](#11-documentation-strategy)
12. [GitHub Strategy](#12-github-strategy)
13. [Developer Experience](#13-developer-experience)
14. [Appendices](#14-appendices)

---

## 1. Vision

### 1.1 Why This Library Exists

Modern .NET applications persist business entities that change over time. Regulations (SOX, GDPR, HIPAA), operational needs (debugging production incidents, understanding customer history), and product features (undo, compare, activity feeds) all require **knowing what changed, when, by whom, and why**—and sometimes **restoring prior state**.

Today's landscape forces a false choice:

| Approach | Strength | Weakness |
|----------|----------|----------|
| **Ad-hoc audit columns** (`CreatedBy`, `ModifiedAt`) | Simple | No history, no rollback, no diff |
| **Database triggers / shadow tables** | Automatic capture | Opaque, hard to query, provider-specific |
| **Full Event Sourcing** | Complete history | High complexity, team skill gap, overkill for CRUD apps |
| **ORM interceptors (EF)** | Integrated | Usually logs raw SQL, not business semantics |
| **Third-party audit libraries** | Faster start | Often EF-only, poor UI, no Git-like mental model |

**BusinessChronicle** fills the gap: **Git-like versioning semantics applied to business entities**, without requiring event sourcing expertise or rewriting applications.

The mental model is deliberate and familiar:

```
Entity State  →  Commit  →  Revision  →  Branch (optional)  →  Tag  →  Rollback
```

Developers think in **commits** and **revisions**, not opaque audit rows.

### 1.2 Problem Statement

BusinessChronicle solves five interconnected problems:

1. **Compliance & accountability** — Immutable, queryable audit trails with actor attribution.
2. **Operational forensics** — Timeline views, diffs, and snapshots for incident investigation.
3. **Safe mutation** — Rollback to known-good revisions without manual SQL restores.
4. **Collaboration context** — Comments, tags, and activity feeds on entity history.
5. **Cross-platform consistency** — Same chronicle semantics in ASP.NET Core APIs, desktop apps, and embedded UIs.

### 1.3 Target Audience

| Persona | Primary Need | Entry Point |
|---------|--------------|-------------|
| **Backend .NET developer** | Drop-in auditing on EF entities | `BusinessChronicle` + `BusinessChronicle.AspNetCore` |
| **Full-stack / Blazor developer** | Timeline UI with minimal setup | `BusinessChronicle.Blazor` |
| **Desktop developer (WPF/WinForms)** | Embedded history viewer | `BusinessChronicle.WPF` / `.WinForms` |
| **Frontend (React) team** | Headless API + React components | `BusinessChronicle.AspNetCore` + `.React` |
| **Platform / framework author** | Extend storage, diff, metadata | `BusinessChronicle.Abstractions` contracts |
| **Enterprise architect** | Scale, compliance, multi-tenant | Configuration, partitioning docs |

**Not targeting:** teams building greenfield CQRS/event-sourced systems (they may still use BusinessChronicle as a read-model adjunct, but it is not the primary audience).

### 1.4 Non-Goals

Explicit boundaries prevent scope creep and architectural erosion:

| Non-Goal | Rationale |
|----------|-----------|
| **Replace Event Sourcing** | Chronicle is revision-based on entity state, not an event log source of truth |
| **Replace Git** | Metaphor only; no merge conflict resolution for code |
| **General-purpose document store** | Optimized for versioned business entities, not arbitrary blobs (though snapshots may store JSON payloads) |
| **Real-time collaboration (OT/CRDT)** | Comments/tags yes; simultaneous editing merge no |
| **Built-in authorization server** | Integrates with ASP.NET Identity, OpenIddict, etc.; does not replace them |
| **ORM replacement** | Works *with* EF Core (primary), extensible to others |
| **Automatic schema migration of consumer databases** | Provides migrations for chronicle tables; consumer entity migrations remain theirs |
| **Multi-master global sync** | Future consideration; v1 is single-primary database |
| **GUI admin portal as required component** | UI packages are optional embeddable components |

### 1.5 Design Philosophy

Eight principles govern every decision:

1. **Convention over configuration** — Sensible defaults; escape hatches when needed.
2. **Semantic over syntactic** — Store *business meaning* of changes, not just SQL diffs.
3. **Async-first, sync-compatible** — All I/O async; sync wrappers only where platform demands.
4. **Trimming & AOT viability** — Core abstractions must not assume reflection-heavy paths.
5. **Progressive disclosure** — `AddBusinessChronicle()` gets 80% there; advanced features are opt-in.
6. **Provider abstraction, EF-first** — EF Core is the reference implementation; others implement contracts.
7. **Zero UI assumptions in core** — UI packages consume public API only.
8. **Fail closed on audit failure** — Configurable, but default preserves integrity (see §6.4).

### 1.6 Long-Term Roadmap

Roadmap is capability-based, not date-based. Phases are ordered by dependency and user value.

```
Phase 0 — Foundation (v0.x)
├── Core domain model (Revision, Commit, Snapshot, Actor)
├── EF Core provider (SQL Server, PostgreSQL, SQLite)
├── Basic capture on SaveChanges
├── Query API (history, get at revision, diff)
└── ASP.NET Core integration + Minimal API endpoints

Phase 1 — Developer Experience (v1.0)
├── Source generators (entity registration, JSON contracts)
├── Roslyn analyzers (misconfiguration detection)
├── Blazor Timeline component
├── CLI (chronicle inspect, export)
└── Stable public API, SemVer guarantee

Phase 2 — Rich History (v1.x)
├── Tags, comments, metadata
├── Activity feed (entity-scoped and global)
├── Rollback (single entity, transactional)
├── Compare (side-by-side, field-level)
└── WPF / WinForms viewers

Phase 3 — Scale & Enterprise (v2.x)
├── MySQL, MongoDB providers
├── Partitioning / archival strategy
├── Background compaction & snapshot policies
├── Multi-tenant isolation patterns
└── React component library (npm package)

Phase 4 — Advanced (v3.x+)
├── Branching model (draft revisions, publish)
├── Cross-entity chronicle queries
├── Federation / read replicas
├── Optional encryption at rest for payloads
└── Pluggable diff engines (binary, JSON Patch, custom)
```

---

## 2. Technology Stack

### 2.1 Runtime & Language

| Technology | Selection | Justification |
|------------|-----------|---------------|
| **.NET 9+** | Primary target; .NET 8 supported on LTS branch | Latest performance, `TimeProvider`, improved generators, better trimming |
| **C# 13+** | `required`, primary constructors, collection expressions | Reduces boilerplate in records and DI |
| **Nullable reference types** | Enabled project-wide (`<Nullable>enable</Nullable>`) | Prevents null bugs in public API |
| **Implicit usings** | Enabled internally; public API uses explicit usings in docs | Cleaner internal code; explicit consumer examples |

**Decision:** Target .NET 9 as minimum for main branch; maintain `release/8.0` branch if demand warrants. Avoid .NET Framework entirely.

**Alternative considered:** .NET Standard 2.0 for max reach. **Rejected** — blocks `System.Text.Json` source gen, `TimeProvider`, trimming, and modern API shapes. BusinessChronicle is forward-looking.

### 2.2 Persistence

| Technology | Role |
|------------|------|
| **EF Core 9+** | Primary ORM; change tracking integration; migrations for chronicle schema |
| **Provider-specific optimizations** | SQL Server (JSON columns, compression), PostgreSQL (JSONB, GIN indexes), SQLite (embedded) |

**Decision:** EF Core as reference provider, not mandatory at abstraction layer. MongoDB provider uses official driver, not EF.

**Alternative considered:** Dapper-only. **Rejected** — loses change-tracking integration, the primary DX win.

### 2.3 Serialization

| Technology | Role |
|------------|------|
| **System.Text.Json** | Snapshot payloads, API responses, metadata |
| **STJ Source Generators** | AOT-friendly, zero-reflection serialization for known types |

**Alternative considered:** Newtonsoft.Json. **Rejected** — heavier, not trim-friendly, not platform default.

### 2.4 ASP.NET Core Integration

| Technology | Role |
|------------|------|
| **Minimal APIs** | First-class endpoint mapping (`MapBusinessChronicle()`) |
| **Endpoint filters** | Actor resolution, correlation ID |
| **ProblemDetails** | Standard error responses |
| **Scalar** | API documentation UI (replacing Swagger UI as default) |
| **OpenAPI** | Schema generation (Scalar consumes OpenAPI) |

**Decision:** Ship Scalar in samples and optional `AddBusinessChronicleApiDocumentation()`. Swashbuckle not required.

### 2.5 Code Generation & Analysis

| Technology | Role |
|------------|------|
| **Roslyn Source Generators** | Entity chronicle registration, JSON type info, change property accessors |
| **Roslyn Analyzers** | Detect missing `EnableBusinessChronicle()`, invalid configs, AOT violations |
| **Analyzer release tracking** | `AnalyzerReleases.Shipped.md` for suppressions |

### 2.6 UI Technologies

| Platform | Stack |
|----------|-------|
| **WPF** | .NET 9+, MVVM-friendly controls, virtualized timeline |
| **WinForms** | .NET 9+, composable `UserControl` |
| **Blazor** | WASM + Server unified components; CSS isolation |
| **React** | TypeScript, headless hooks + styled components, distributed via npm |

### 2.7 Testing & Quality

| Technology | Role |
|------------|------|
| **xUnit** | Unit/integration tests |
| **Verify** | Snapshot testing for diffs/serialization |
| **Testcontainers** | Real database integration tests |
| **BenchmarkDotNet** | Performance regression suite |
| **Nuke** or **Microsoft.Build.Traversal** | CI build orchestration |

**Decision:** Nuke for complex multi-target builds; simple `build.sh`/`build.ps1` wrappers for contributors.

### 2.8 Native AOT & Trimming

| Requirement | Approach |
|-------------|----------|
| **Trim-safe core** | `[DynamicallyAccessedMembers]`, source gen over reflection |
| **AOT-compatible API surface** | No runtime expression compile in default paths |
| **Opt-in AOT sample** | `BusinessChronicle.Samples.NativeAot` validates compatibility |

---

## 3. Solution Structure

### 3.1 Repository Layout

```
BusinessChronicle/
├── src/
│   ├── BusinessChronicle/                          # Main NuGet (facade + core impl)
│   ├── BusinessChronicle.Abstractions/             # Contracts, options, primitives
│   ├── BusinessChronicle.Core/                     # Domain logic (pure)
│   ├── BusinessChronicle.EntityFrameworkCore/      # EF Core provider
│   ├── BusinessChronicle.EntityFrameworkCore.SqlServer/
│   ├── BusinessChronicle.EntityFrameworkCore.PostgreSql/
│   ├── BusinessChronicle.EntityFrameworkCore.Sqlite/
│   ├── BusinessChronicle.AspNetCore/               # DI, middleware, Minimal API
│   ├── BusinessChronicle.Api/                      # Optional standalone API host (reference)
│   ├── BusinessChronicle.Cli/                      # Global tool: dotnet-chronicle
│   ├── BusinessChronicle.Blazor/                   # Razor class library
│   ├── BusinessChronicle.Wpf/                      # WPF controls
│   ├── BusinessChronicle.WinForms/                 # WinForms controls
│   ├── BusinessChronicle.React/                    # JS/TS bridge + OpenAPI client gen
│   ├── BusinessChronicle.SourceGenerators/
│   ├── BusinessChronicle.Analyzers/
│   └── BusinessChronicle.Internal/                 # Shared internals (InternalsVisibleTo only)
├── tests/
│   ├── BusinessChronicle.Core.Tests/
│   ├── BusinessChronicle.EntityFrameworkCore.Tests/
│   ├── BusinessChronicle.AspNetCore.Tests/
│   ├── BusinessChronicle.Analyzers.Tests/
│   ├── BusinessChronicle.SourceGenerators.Tests/
│   ├── BusinessChronicle.Testing/                  # Test helpers/fakes (published for consumers)
│   ├── BusinessChronicle.Benchmarks/
│   └── BusinessChronicle.IntegrationTests/
├── samples/
│   ├── BusinessChronicle.Samples.MinimalApi/
│   ├── BusinessChronicle.Samples.Mvc/
│   ├── BusinessChronicle.Samples.Blazor/
│   ├── BusinessChronicle.Samples.Wpf/
│   ├── BusinessChronicle.Samples.WinForms/
│   ├── BusinessChronicle.Samples.NativeAot/
│   └── BusinessChronicle.Samples.React/            # SPA consuming API
├── docs/
│   ├── ARCHITECTURE.md                             # This document
│   ├── api/                                        # Generated API docs
│   ├── cookbook/
│   └── ...
├── benchmarks/
├── .github/
├── build/
├── Directory.Build.props
├── Directory.Packages.props                        # Central package management
├── global.json
├── LICENSE
├── README.md
└── BusinessChronicle.sln
```

### 3.2 Project Responsibilities

#### BusinessChronicle.Abstractions
- **Purpose:** Stable, minimal-dependency public contracts.
- **Contains:** `IChronicleStore`, `IRevisionReader`, `IRevisionWriter`, `IActorResolver`, `IChronicleDiffEngine`, options classes, domain IDs (`RevisionId`, `CommitId`, `EntityRef`), event argument types.
- **Dependencies:** None (only BCL).
- **Shipped:** Yes, as transitive dependency (not typically referenced directly).

#### BusinessChronicle.Core
- **Purpose:** Domain logic independent of EF Core and ASP.NET.
- **Contains:** Revision graph, diff orchestration, snapshot policies, commit bundling, metadata merge rules, timeline aggregation, rollback planning (validation, not execution).
- **Dependencies:** `Abstractions` only.
- **Shipped:** Transitive via main package.

#### BusinessChronicle (Main Package)
- **Purpose:** Facade assembly consumers reference. Wires default implementations.
- **Contains:** `ChronicleBuilder`, `ServiceCollectionExtensions`, `ChronicleOptions`, public entry points, default in-memory store for testing.
- **Dependencies:** `Abstractions`, `Core`, `EntityFrameworkCore` (default).
- **Shipped:** **Primary NuGet.**

#### BusinessChronicle.EntityFrameworkCore
- **Purpose:** EF Core integration — interceptors, model configuration, chronicle DbContext extensions, migrations helpers.
- **Contains:** `SaveChangesInterceptor`, `ModelBuilder` extensions, `ChronicleDbContext` optional base, value converters for IDs.
- **Dependencies:** `Abstractions`, `Core`, EF Core.
- **Shipped:** Transitive via main package; also direct ref for advanced scenarios.

#### BusinessChronicle.EntityFrameworkCore.{Provider}
- **Purpose:** Provider-specific index hints, JSON storage, bulk operations.
- **Dependencies:** `EntityFrameworkCore`, provider package.
- **Shipped:** Transitive when provider detected, or explicit meta-package.

**Decision:** Separate provider projects vs. single EF project with `#if`. **Selected: Separate projects** — keeps trimming clean, avoids pulling all providers.

#### BusinessChronicle.AspNetCore
- **Purpose:** HTTP integration.
- **Contains:** `AddBusinessChronicle()`, actor resolution from `ClaimsPrincipal`, correlation middleware, `MapChronicleEndpoints()`, ProblemDetails mapping.
- **Dependencies:** `Abstractions`, `Core`, `BusinessChronicle`, ASP.NET Core framework.
- **Shipped:** Separate NuGet.

#### BusinessChronicle.Blazor / Wpf / WinForms
- **Purpose:** Embeddable UI components.
- **Contains:** Timeline, diff viewer, comment panel, tag editor.
- **Dependencies:** `Abstractions`, `BusinessChronicle` (or HTTP client for remote mode).
- **Shipped:** Separate NuGet per platform.

#### BusinessChronicle.React
- **Purpose:** npm package (`@business-chronicle/react`) + optional .NET wrapper for OpenAPI client generation.
- **Dependencies:** Logically depends on API contract, not .NET Core.
- **Shipped:** npm + optional NuGet meta-package for client gen.

#### BusinessChronicle.SourceGenerators
- **Purpose:** Compile-time generation.
- **Contains:** `[ChronicleEntity]` handler, JSON context generation, property change accessor tables.
- **Shipped:** Analyzer package (development dependency).

#### BusinessChronicle.Analyzers
- **Purpose:** Compile-time diagnostics.
- **Contains:** BC0001–BC9999 rule set.
- **Shipped:** Analyzer package.

#### BusinessChronicle.Testing
- **Purpose:** Fakes, in-memory store, test assertions (`AssertRevisionCount`, `AssertDiff`).
- **Shipped:** Separate NuGet for consumer tests.

#### BusinessChronicle.Cli
- **Purpose:** `dotnet tool install -g BusinessChronicle.Cli`
- **Shipped:** Global tool NuGet.

#### BusinessChronicle.Internal
- **Purpose:** Shared code between generators and runtime (not public).
- **Dependencies:** Restricted.
- **Shipped:** No.

#### Non-shipping Projects
- `BusinessChronicle.Api` — reference standalone host for samples.
- `BusinessChronicle.Benchmarks` — performance.
- `BusinessChronicle.Samples.*` — examples.
- `BusinessChronicle.Playground` — manual dev scratchpad (gitignored experiments).
- `BusinessChronicle.Documentation` — DocFX or Markdown site generator project.

### 3.3 Solution Folders (Visual Studio)

Organize solution into folders: **Core**, **Data**, **Web**, **Desktop**, **UI**, **Tooling**, **Tests**, **Samples** — mirrors mental model, not physical nesting.

---

## 4. Dependency Rules

### 4.1 Dependency Graph

```
                    ┌─────────────────────────┐
                    │  Samples / Playground   │
                    └───────────┬─────────────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        │                       │                       │
        ▼                       ▼                       ▼
┌───────────────┐     ┌─────────────────┐     ┌─────────────────┐
│ Blazor/WPF/   │     │  AspNetCore     │     │  Cli            │
│ WinForms/React│     │                 │     │                 │
└───────┬───────┘     └────────┬────────┘     └────────┬────────┘
        │                      │                       │
        └──────────────────────┼───────────────────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │  BusinessChronicle  │  (facade)
                    └──────────┬──────────┘
                               │
              ┌────────────────┼────────────────┐
              │                │                │
              ▼                ▼                ▼
    ┌─────────────────┐ ┌───────────┐ ┌──────────────────┐
    │ EntityFramework │ │   Core    │ │  Abstractions    │
    │ Core (+providers)│ │           │ │                  │
    └────────┬────────┘ └─────┬─────┘ └──────────────────┘
             │                │
             └────────────────┘
                      │
                      ▼
              Abstractions (leaf)

Analyzers / SourceGenerators → Abstractions, Internal (no runtime deps)
Testing → Abstractions, Core (published); references EF optionally
```

### 4.2 Allowed References

| Project | May Reference |
|---------|---------------|
| `Abstractions` | BCL only |
| `Core` | `Abstractions` |
| `EntityFrameworkCore*` | `Abstractions`, `Core`, EF Core |
| `BusinessChronicle` | `Abstractions`, `Core`, `EntityFrameworkCore` |
| `AspNetCore` | `BusinessChronicle`, ASP.NET Core |
| UI projects | `Abstractions`, `BusinessChronicle` OR HTTP client only (remote mode) |
| `SourceGenerators` | `Internal`, Roslyn |
| `Analyzers` | `Internal`, Roslyn |
| `Testing` | `Abstractions`, `Core`, optional EF |
| Tests | Anything under test + `Testing` |
| Samples | Public packages only (dogfooding) |

### 4.3 Forbidden References (Hard Rules)

| Rule | Reason |
|------|--------|
| `Abstractions` → anything but BCL | Foundation purity |
| `Core` → EF Core, ASP.NET, UI | Domain isolation |
| `Core` → `BusinessChronicle` (facade) | No upward deps |
| `EntityFrameworkCore` → AspNetCore, UI | Layer violation |
| `Analyzers/Generators` → Core runtime impl | Generator isolation |
| Any project → `Samples`, `Playground`, `Benchmarks` | No production coupling |
| UI → `EntityFrameworkCore` directly | Must go through abstractions/facade |
| Public packages → `Internal` (except IVT) | Encapsulation |

### 4.4 InternalsVisibleTo

`Internal` is visible only to:
- `BusinessChronicle.SourceGenerators`
- `BusinessChronicle.Analyzers`
- Test assemblies (`*.Tests`)

Never expose `Internal` to UI or consumer code.

---

## 5. Package Strategy

### 5.1 Design Goal

**Installation in two lines for the common case:**

```csharp
builder.Services.AddBusinessChronicle();
// ...
modelBuilder.Entity<Order>().EnableBusinessChronicle();
```

### 5.2 Published NuGet Packages

| Package | Audience | Contents |
|---------|----------|----------|
| **`BusinessChronicle`** | Everyone (required) | Facade, Core, Abstractions, EF Core base, SQL Server/SQLite/PostgreSQL pulled via dependencies |
| **`BusinessChronicle.AspNetCore`** | Web apps | Middleware, Minimal API, actor resolution |
| **`BusinessChronicle.Blazor`** | Blazor apps | Razor components |
| **`BusinessChronicle.Wpf`** | WPF apps | Timeline controls |
| **`BusinessChronicle.WinForms`** | WinForms apps | Viewer controls |
| **`BusinessChronicle.Testing`** | Test authors | Fakes, helpers |
| **`BusinessChronicle.Cli`** | DevOps/local inspect | Global tool |
| **`BusinessChronicle.Analyzers`** | All (auto-ref) | Analyzers + source generators |

**Not separate NuGets (transitive):**
- `Abstractions`, `Core`, `EntityFrameworkCore`, provider projects — implementation detail inside `BusinessChronicle`.

**Decision:** Monolithic core package vs. many packages.

| Option | Pros | Cons |
|--------|------|------|
| Many packages (`Core`, `EF`, each provider) | Granular deps | Configuration hell, version mismatch |
| Single `BusinessChronicle` + platform add-ons | Simple install | Slightly larger download |
| **Selected** | **Single main + platform** | Best DX |

### 5.3 Meta-Package vs. Facade Assembly

**Selected:** Facade assembly (`BusinessChronicle`) that references implementation projects, published as one NuGet with bundled dependencies. Not a meta-package that only pulls refs — the facade contains `AddBusinessChronicle()` and public surface.

### 5.4 npm Package (React)

| Package | Registry |
|---------|----------|
| `@business-chronicle/react` | npmjs.com |
| `@business-chronicle/core` | Shared types/utilities |

.NET `BusinessChronicle.React` NuGet optionally includes MSBuild targets to fetch/generate TypeScript client from OpenAPI.

### 5.5 Versioning Alignment

All packages share **exact version** per release (e.g., `1.2.0`). Single tag, single GitHub release, matrix publish.

### 5.6 Prerelease Channels

| Channel | Tag | Purpose |
|---------|-----|---------|
| Stable | `1.0.0` | Production |
| RC | `1.0.0-rc.1` | Final testing |
| Preview | `1.1.0-preview.1` | Early adopters |
| Nightly | `1.1.0-nightly.20260701` | CI feed (optional) |

---

## 6. Public API Philosophy

### 6.1 API Design Principles

1. **Fluent configuration** — method chaining on builders.
2. **Discoverable entry points** — everything starts from `AddBusinessChronicle`, `EnableBusinessChronicle`, `Chronicle`.
3. **Options pattern** — `ChronicleOptions`, `ChronicleEntityOptions`, `ChronicleStorageOptions`.
4. **Extension methods over inheritance** — consumers extend via extensions, not base classes.
5. **Explicit entity opt-in** — chronicle is never global-by-surprise on all entities.

### 6.2 Registration API (Conceptual)

```csharp
// ASP.NET Core — single entry
builder.Services.AddBusinessChronicle(options =>
{
    options.DefaultActorResolver = sp => sp.GetRequiredService<IHttpContextAccessor>();
    options.Storage.Provider = ChronicleProvider.PostgreSql;
    options.OnCommitFailure = CommitFailureBehavior.FailTransaction; // default
});

// EF Core — per entity
modelBuilder.Entity<Order>(entity =>
{
    entity.EnableBusinessChronicle(options =>
    {
        options.IgnoreProperties(o => o.InternalCache);
        options.SnapshotFrequency = SnapshotFrequency.EveryRevision; // or EveryNRevisions(10)
        options.Metadata.IncludeRelated(typeof(OrderLine));
    });
});

// Optional global defaults
modelBuilder.ConfigureBusinessChronicleDefaults(options => { ... });
```

### 6.3 Query & Command API (Conceptual)

Static accessor pattern for ergonomics in services:

```csharp
// Injected alternative: IChronicleSession
var history = await Chronicle.For<Order>(orderId)
    .GetHistoryAsync(cancellationToken);

var atRevision = await Chronicle.For<Order>(orderId)
    .AtRevisionAsync(revisionId, cancellationToken);

var diff = await Chronicle.For<Order>(orderId)
    .CompareAsync(revisionA, revisionB, cancellationToken);

await Chronicle.For<Order>(orderId)
    .RollbackAsync(revisionId, cancellationToken);
```

**Decision:** `Chronicle.For<T>()` static vs. only `IChronicleSession`.

| Option | Verdict |
|--------|---------|
| Static only | Hard to test |
| Interface only | Verbose |
| **Both** | Static delegates to scoped session; **selected** |

### 6.4 Commit Failure Behavior

| Behavior | Use Case |
|----------|----------|
| `FailTransaction` (default) | Compliance — change cannot persist without audit |
| `LogAndContinue` | Best-effort auditing (explicit opt-in) |
| `QueueForRetry` | Background recovery |

### 6.5 Minimal API Endpoints

```csharp
app.MapBusinessChronicle(); // maps /api/chronicle/{entityType}/{id}/history, etc.

// Or granular:
app.MapChronicleHistory<Order>("/orders/{orderId}/history");
app.MapChronicleDiff<Order>("/orders/{orderId}/diff");
app.MapChronicleRollback<Order>("/orders/{orderId}/rollback");
```

Authorization: integrates with `[Authorize]` policies; default policy requires authentication for rollback.

### 6.6 UI Registration

```csharp
// Blazor
builder.Services.AddBusinessChronicleUI(); // registers services + embedded JS if needed

// WPF App.xaml.cs
services.AddBusinessChronicleUI(forWpf: true);
```

### 6.7 Naming Conventions (Public API)

| Pattern | Example |
|---------|---------|
| Entry extensions | `AddBusinessChronicle`, `EnableBusinessChronicle` |
| Options | `ChronicleXxxOptions` |
| Interfaces | `IChronicleXxx` (avoid `IChronicleService` — too generic) |
| Async methods | `XxxAsync` suffix always |
| Cancellation | Last parameter `CancellationToken cancellationToken = default` |

### 6.8 Configuration Hierarchy

```
appsettings.json
  Chronicle:Storage:Provider
  Chronicle:Storage:ConnectionStringName
  Chronicle:Defaults:SnapshotFrequency
  Chronicle:Entities:Order:IgnoreProperties

Environment variables (Chronicle__Storage__Provider)
Code (AddBusinessChronicle callback) — highest precedence
```

---

## 7. Coding Standards

### 7.1 General

- **Language version:** Latest stable C#; pinned in `Directory.Build.props`.
- **Treat warnings as errors** in CI (`TreatWarningsAsErrors=true`).
- **EditorConfig** enforced — Microsoft defaults + project custom rules.
- **File-scoped namespaces** everywhere.
- **Primary constructors** for simple DI services; explicit when logic grows.

### 7.2 Naming

| Element | Convention | Example |
|---------|------------|---------|
| Namespace | `BusinessChronicle.{Area}.{SubArea}` | `BusinessChronicle.Core.Diff` |
| Interfaces | `I` prefix | `IRevisionStore` |
| Async types | No `Async` in type names | `RevisionReader` with `ReadAsync` |
| Options | `{Feature}Options` | `ChronicleEntityOptions` |
| Exceptions | `{Problem}Exception` | `RollbackConflictException` |
| Internal types | no prefix | `RevisionGraphBuilder` |

### 7.3 Folder Organization (Per Project)

```
BusinessChronicle.Core/
├── Diff/
├── Revisions/
├── Snapshots/
├── Timeline/
├── Rollback/
├── Metadata/
└── Internal/
```

- One public type per file (exceptions: nested enums, small records).
- `partial class` for source generator companions (`Order.Chronicle.g.cs`).
- `Internal/` folder for non-public helpers; never in public namespace.

### 7.4 Visibility

- **Default to internal**; public only when in Abstractions or intentional facade.
- **Sealed** classes unless designed for inheritance (`ChronicleBuilder` unsealed for advanced extension).
- **`readonly struct`** for IDs (`RevisionId`, `CommitId`).

### 7.5 Documentation Comments

- XML docs on **all public APIs** in Abstractions and facade.
- `<summary>`, `<param>`, `<returns>`, `<exception>` required.
- Examples via `<example>` where non-obvious.
- Enable CS1591 as warning (not error) in Abstractions; error in released packages.

### 7.6 Extension Methods

- Static class per extended type: `EntityTypeBuilderChronicleExtensions`.
- Null checks via `ArgumentNullException.ThrowIfNull`.

### 7.7 Dependency Injection

- Register via `IServiceCollection` extensions only in AspNetCore/facade projects.
- Prefer `TryAddScoped` for defaults; allow override.
- Key services:

| Service | Lifetime |
|---------|----------|
| `IChronicleSession` | Scoped |
| `IChronicleStore` | Scoped |
| `IActorResolver` | Scoped |
| `IChronicleDiffEngine` | Singleton |
| `ChronicleOptions` | Singleton (options monitor) |

### 7.8 Error Handling

- Domain exceptions in `BusinessChronicle.Abstractions.Exceptions`.
- Map to HTTP status in AspNetCore layer only.
- Never swallow exceptions in interceptors without configurable policy.

---

## 8. Performance Strategy

### 8.1 Scale Targets (Design Constraints)

| Metric | Target |
|--------|--------|
| Entities under chronicle | Millions |
| Revisions per entity | Thousands (hot); millions (archived) |
| Commits per second (single node) | 1,000+ sustained |
| History query p95 | < 50ms (indexed, 100 revisions) |
| Diff computation p95 | < 10ms (typical entity) |

### 8.2 Reflection Avoidance

| Technique | Application |
|-----------|-------------|
| Source generators | Property accessors, entity registration tables, JSON serialization |
| Compiled delegates | Change detection at startup (one-time compile, cached) |
| `IChronicleEntityDescriptor`** | Precomputed metadata per entity type |

**Alternative considered:** Pure reflection on `SaveChanges`. **Rejected** — too slow at scale.

### 8.3 Change Detection Pipeline

```
SaveChanges triggered
    → Interceptor captures EF ChangeTracker entries (filtered to chronicle-enabled)
    → Descriptor table maps PropertyEntry → semantic field name
    → Delta computed (old/new values from original/current)
    → Commit batch assembled
    → Async write to chronicle store (same transaction when possible)
```

### 8.4 Caching Strategy

| Cache | Scope | Invalidation |
|-------|-------|--------------|
| Entity descriptors | Process / singleton | App restart; hot reload in dev |
| Recent revision HEAD pointer | Scoped / distributed | On commit |
| Diff results | Optional memory cache | TTL or revision-bound |
| Timeline page slices | Response cache (HTTP) | ETag based on latest revision |

Use `IMemoryCache` default; `IDistributedCache` optional for multi-instance.

### 8.5 Object Pooling

- Pool `DeltaBuffer`, `StringBuilder`, `Utf8JsonWriter` for snapshot serialization.
- `ArrayPool<byte>` for large JSON payloads.
- Do not pool EF entities or DbContext.

### 8.6 ValueTask

- Use `ValueTask`/`ValueTask<T>` on hot paths where often synchronous (cache hit, in-memory test store).
- Return `Task<T>` on public API for consistency unless benchmark proves benefit.

### 8.7 Async Best Practices

- `ConfigureAwait(false)` in library code.
- No `async void` except UI event handlers in WPF/WinForms.
- Channel-based background queue for `QueueForRetry` failure mode.
- `CancellationToken` propagated end-to-end.

### 8.8 Database Performance

- **Append-only revision table** — clustered index on `(EntityType, EntityId, RevisionNumber DESC)`.
- **Snapshot table** — optional denormalized JSON at intervals to avoid replaying all deltas.
- **Partitioning** — by time or tenant for archival (Phase 3).
- **Bulk insert** — batch commits in single round-trip where provider supports.

### 8.9 Background Processing

| Job | Purpose |
|-----|---------|
| Snapshot compaction | Create snapshot every N revisions |
| Archival | Move cold revisions to archive storage |
| Retry queue | Process failed commit writes |
| Index maintenance | Provider-specific |

Use `IHostedService` in AspNetCore; `System.Threading.Channels` in core.

### 8.10 Thread Safety

- Stores must document thread safety; default EF implementation is **not thread-safe** (scoped to DbContext).
- Immutable revision records — safe to share.
- Singleton diff engine must be thread-safe.

### 8.11 Benchmarking

- `BusinessChronicle.Benchmarks` with BenchmarkDotNet.
- Categories: Change capture, diff, serialization, history query.
- CI: run on `main` weekly; fail on >10% regression without approval label.

---

## 9. Extensibility

### 9.1 Extension Model Overview

BusinessChronicle uses **provider + plugin** pattern, not a monolithic plugin host with MEF.

```
┌─────────────────────────────────────────────────────────┐
│                    Application                          │
├─────────────────────────────────────────────────────────┤
│  UI Extensions (Blazor/WPF/React renderers)           │
├─────────────────────────────────────────────────────────┤
│  ASP.NET Core (actor resolvers, authorization)          │
├─────────────────────────────────────────────────────────┤
│  BusinessChronicle Facade                               │
├──────────────┬──────────────┬──────────────┬────────────┤
│ IChronicle   │ IChronicle   │ IChronicle   │ IChronicle │
│ Store        │ DiffEngine   │ Snapshot     │ Metadata   │
│ Provider     │              │ Serializer   │ Provider   │
└──────────────┴──────────────┴──────────────┴────────────┘
```

### 9.2 First-Class Extension Points

| Extension Point | Interface | Purpose |
|-----------------|-----------|---------|
| Storage provider | `IChronicleStore` | MongoDB, custom SQL, cloud |
| Diff engine | `IChronicleDiffEngine` | JSON Patch, binary, domain-specific |
| Snapshot serializer | `IChronicleSnapshotSerializer` | Custom formats, encryption |
| Actor resolver | `IActorResolver` | System user, service accounts, impersonation |
| Metadata enricher | `IChronicleMetadataEnricher` | Correlation ID, tenant, custom tags |
| Commit pipeline hook | `IChronicleCommitFilter` | Redact PII before persist |
| Timeline renderer | `IChronicleTimelineRenderer` (UI) | Custom visualization |
| Archival provider | `IChronicleArchivalStore` | S3, Azure Blob for cold storage |

### 9.3 Registration

```csharp
builder.Services.AddBusinessChronicle()
    .UseStore<MongoChronicleStore>()
    .UseDiffEngine<JsonPatchDiffEngine>()
    .AddMetadataEnricher<TenantMetadataEnricher>();
```

**Decision:** Fluent builder on top of DI vs. pure `services.AddSingleton<IChronicleStore, ...>`.

**Selected:** Both — fluent wraps standard DI registration for discoverability.

### 9.4 Plugin Discovery (Optional, Phase 3)

- Assembly attribute `[assembly: ChroniclePlugin(typeof(MyStore))]`
- Source generator collects plugins at compile time (AOT-safe).
- **Not** runtime directory scanning (breaks trimming/AOT).

### 9.5 Custom Storage Contract

`IChronicleStore` minimum operations:

- `AppendCommitAsync`
- `GetRevisionAsync` / `GetRevisionRangeAsync`
- `GetHeadRevisionAsync`
- `WriteSnapshotAsync` / `ReadSnapshotAsync`
- Transaction participation hook (`BeginChronicleTransactionAsync`)

EF implementation participates in ambient `DbContext` transaction.

### 9.6 Third-Party Package Naming

Community packages: `BusinessChronicle.Contrib.{Name}` — not official; listed in docs.

---

## 10. UI Strategy

### 10.1 Design Principles

1. **Embed, don't mandate** — components drop into existing apps.
2. **Local-first, remote-capable** — work in-process (desktop) or via API (React).
3. **Headless core** — logic in shared services; UI is thin.
4. **Theming** — CSS variables (Blazor/React), ResourceDictionary (WPF), styles (WinForms).
5. **Virtualization** — timelines must virtualize for thousands of revisions.

### 10.2 Architecture Layers

```
┌──────────────────────────────────────┐
│  Platform Component (Timeline UI)    │
├──────────────────────────────────────┤
│  ChronicleViewModel / Hooks          │  ← platform-specific binding
├──────────────────────────────────────┤
│  IChronicleViewService                 │  ← shared contract (Abstractions)
├──────────────────────────────────────┤
│  IChronicleSession / HTTP Client       │
└──────────────────────────────────────┘
```

### 10.3 WPF

```xml
<bc:Timeline EntityType="{x:Type local:Order}"
             EntityId="{Binding OrderId}"
             ShowDiff="True"
             ShowComments="True" />
```

- Control resolves `IChronicleViewService` from `Application.Current` DI or creates local client.
- Templates for revision item, actor badge, tag chip — customizable via `DataTemplate`.
- Zero page creation: optional `ChronicleWindow` for standalone debug.

### 10.4 WinForms

```csharp
var viewer = new ChronicleViewer
{
    EntityType = typeof(Order),
    EntityId = orderId,
    Dock = DockStyle.Fill
};
```

- Designer-friendly; events: `RevisionSelected`, `RollbackRequested`.

### 10.5 Blazor

```razor
<ChronicleTimeline TEntity="Order" EntityId="@OrderId"
                   OnRollback="HandleRollback" />
```

- InteractiveServer / WASM compatible.
- CSS isolation in `ChronicleTimeline.razor.css`.
- Optional `[ChroniclePage]` attribute generates full history page route.

### 10.6 React

```tsx
<ChronicleTimeline
  entityType="Order"
  entityId={orderId}
  apiBaseUrl="/api/chronicle"
  onRollback={handleRollback}
/>
```

- npm package ships ES modules + TypeScript types.
- Hooks: `useChronicleHistory`, `useChronicleDiff`.
- Styling: unstyled primitives + optional `@business-chronicle/theme-default`.

### 10.7 Remote vs. Local Mode

| Mode | When |
|------|------|
| **Local** | Desktop apps with direct DB/service access |
| **Remote** | React SPA, Blazor WASM — calls `MapBusinessChronicle` API |

Configuration: `ChronicleUI:Mode = Local | Remote`.

### 10.8 Authorization in UI

- Components accept `CanRollback`, `CanComment` flags OR query `IChronicleAuthorizationService`.
- Never bypass server authorization — UI hides actions; API enforces.

---

## 11. Documentation Strategy

### 11.1 Documentation Site Structure

```
docs/
├── index.md                    → Overview, quick start
├── getting-started/
│   ├── installation.md
│   ├── first-entity.md
│   └── aspnetcore-setup.md
├── architecture/
│   ├── overview.md             → Link to ARCHITECTURE.md concepts (summary)
│   ├── domain-model.md
│   ├── data-model.md
│   └── security.md
├── guides/
│   ├── rollback.md
│   ├── multi-tenant.md
│   ├── performance-tuning.md
│   └── compliance.md
├── cookbook/
│   ├── ignore-sensitive-fields.md
│   ├── custom-actor-resolution.md
│   └── ...
├── api/                        → DocFX generated
├── migration/
│   ├── from-audit-net.md
│   └── v1-to-v2.md
├── faq.md
├── contributing.md
├── roadmap.md
└── benchmarks.md
```

### 11.2 README (GitHub)

Sections:
1. Tagline + badges (build, NuGet, license, coverage)
2. 30-second value proposition
3. Quick start (copy-paste)
4. Feature matrix (phase indicators)
5. Links to docs site, samples, Discord/GitHub Discussions
6. Contributing pointer

### 11.3 API Reference

- **DocFX** from XML comments + markdown concept docs.
- Published to GitHub Pages (`https://businesschronicle.github.io/docs` or custom domain).
- Every public type has minimal example.

### 11.4 Samples as Documentation

Each sample README explains:
- What it demonstrates
- How to run
- Key code walkthrough (3–5 bullets)

Samples are CI-built to prevent rot.

### 11.5 Cookbook Format

Problem → Solution → Complete code → Caveats. One recipe per page.

### 11.6 Migration Guides

Triggered on major versions; include:
- Breaking changes table
- Automated migration tool if feasible (analyzer codemods)

---

## 12. GitHub Strategy

### 12.1 Repository Presentation

| Element | Standard |
|---------|----------|
| **License** | MIT (permissive, enterprise-friendly) |
| **CODE_OF_CONDUCT** | Contributor Covenant |
| **SECURITY.md** | Responsible disclosure |
| **SUPPORT.md** | Where to ask questions |
| **CHANGELOG.md** | Keep a Changelog format |
| **README** | Professional, concise |
| **Brand** | Logo + consistent color (docs + NuGet icon) |

### 12.2 Branch Strategy

| Branch | Purpose |
|--------|---------|
| `main` | Latest stable development toward next release |
| `release/x.y` | Servicing patches |
| `feature/*` | Feature work |
| `fix/*` | Bug fixes |

Trunk-based with short-lived feature branches; RC tags from `main`.

### 12.3 Labels

**Type:** `bug`, `enhancement`, `documentation`, `sample`, `performance`, `breaking-change`

**Area:** `area:core`, `area:ef`, `area:aspnet`, `area:blazor`, `area:wpf`, `area:react`, `area:tooling`

**Priority:** `P0-critical`, `P1-high`, `P2-medium`, `P3-low`

**Status:** `needs-triage`, `needs-design`, `good first issue`, `help wanted`

**Community:** `question`, `duplicate`, `wontfix`

### 12.4 Milestones

Align with roadmap phases (e.g., `v1.0.0`, `v1.1.0 Tags & Comments`). Close milestone when shipped.

### 12.5 GitHub Projects

Single **Product Board**:
- Backlog → Design → Ready → In Progress → Review → Done

Automation: PR linked issues move columns.

### 12.6 Issue Templates

| Template | Fields |
|----------|--------|
| **Bug Report** | Version, provider, repro steps, expected/actual |
| **Feature Request** | Problem, proposed API, alternatives |
| **Provider Request** | Database, expected scale |
| **Question** | Redirect to Discussions optionally |

### 12.7 Pull Request Template

- Summary
- Related issue
- Type (bug/feature/docs)
- Breaking change checklist
- Test plan
- Benchmark impact (if perf-sensitive)

### 12.8 Release Strategy

1. Feature freeze → RC branch/tag
2. Integration tests + benchmarks green
3. Update CHANGELOG
4. Tag `vX.Y.Z`
5. GitHub Release with notes (auto-generated + curated)
6. NuGet publish (signed packages)
7. npm publish for React (lockstep version)

### 12.9 Semantic Versioning

- **MAJOR:** Breaking public API or schema migration requiring consumer action
- **MINOR:** New features, backward compatible
- **PATCH:** Bug fixes

Analyzer rule changes that break build → MAJOR or documented exception in minor with fix guide.

### 12.10 GitHub Actions Workflows

| Workflow | Trigger | Actions |
|----------|---------|---------|
| `ci.yml` | PR, push main | Build, test matrix (Windows/Linux/macOS), analyzers |
| `integration.yml` | PR, nightly | Testcontainers SQL Server + PostgreSQL |
| `benchmarks.yml` | Weekly, manual | BenchmarkDotNet, upload results |
| `docs.yml` | Push main, tags | DocFX → GitHub Pages |
| `publish.yml` | Tag `v*` | NuGet + npm + GitHub release |
| `codeql.yml` | PR, schedule | Security analysis |
| `stale.yml` | Daily | Stale issue management |

### 12.11 NuGet Publishing

- Package signing (Author signing certificate)
- Symbol packages (`snupkg`) to Symbol Server
- README and icon embedded in all packages
- `PackageValidation` enabled against previous version

---

## 13. Developer Experience

### 13.1 The "Five-Minute" Experience

```bash
dotnet add package BusinessChronicle
dotnet add package BusinessChronicle.AspNetCore
```

```csharp
builder.Services.AddBusinessChronicle();
builder.Services.AddDbContext<AppDbContext>(...);

// In AppDbContext.OnModelCreating:
modelBuilder.Entity<Order>().EnableBusinessChronicle();
```

Run app → changes audited. **No chronicle-specific DbContext required for basic use.**

### 13.2 Sensible Defaults

| Setting | Default |
|---------|---------|
| Snapshot frequency | Every 10 revisions |
| Ignored properties | None (analyzer warns on sensitive names like `Password`) |
| Actor | `Environment.UserName` / HTTP user if AspNetCore |
| Commit failure | Fail transaction |
| Storage | Same database as DbContext |
| API endpoints | Off until `MapBusinessChronicle()` |

### 13.3 Analyzer-Guided Development

| Rule | Message |
|------|---------|
| BC001 | Entity has changes but `EnableBusinessChronicle()` not called |
| BC002 | Sensitive property name not ignored |
| BC003 | Chronicle requires primary key configuration |
| BC004 | Async rollback called from sync context incorrectly |
| BC005 | AOT-incompatible custom serializer detected |

### 13.4 Source Generator Magic

- `[ChronicleEntity]` attribute auto-registers entity (alternative to fluent).
- Generates `IChronicleEntityDescriptor` implementation.
- Generates STJ context for snapshot types.

### 13.5 CLI Tooling

```bash
dotnet chronicle list --entity Order --id 123
dotnet chronicle diff --entity Order --id 123 --from rev1 --to rev2
dotnet chronicle export --format json --output ./audit-export
dotnet chronicle doctor  # validates configuration
```

### 13.6 Testing DX

```csharp
using BusinessChronicle.Testing;

await using var ctx = ChronicleTestContext.Create();
ctx.EnableChronicle<Order>();
// ... mutate order ...
await ctx.SaveChangesAsync();

ctx.ShouldHaveRevisionCount<Order>(orderId, 3);
ctx.ShouldHaveChange<Order>(orderId, r => r.Status, from: "Draft", to: "Submitted");
```

### 13.7 Error Messages

- Actionable, include documentation link.
- Example: *"BusinessChronicle could not resolve actor for commit. Register `IActorResolver` or set `ChronicleOptions.DefaultActor`. See: https://..."*

### 13.8 IntelliSense & Discoverability

- Extension methods in `BusinessChronicle` root namespace (single `global using` optional).
- `[EditorBrowsable]` hides advanced APIs.
- Package readme shows top 3 scenarios.

### 13.9 Template Pack (Future)

```bash
dotnet new install BusinessChronicle.Templates
dotnet new bc-webapi -n MyApp
```

---

## 14. Appendices

### Appendix A — Domain Model (Conceptual)

```
EntityRef (Type + Id)
    └── Commit (batch of entity changes in one SaveChanges / explicit commit)
            ├── Actor (who)
            ├── Timestamp (when)
            ├── CorrelationId (why/context)
            ├── Metadata (tags, comments, custom JSON)
            └── Revision[] (per entity)
                    ├── RevisionId
                    ├── RevisionNumber (monotonic per entity)
                    ├── ParentRevisionId (linear chain; branches later)
                    ├── Delta | Snapshot
                    └── Message (optional commit message)
```

### Appendix B — Physical Data Model (EF Core Reference)

**Tables (prefix `BC_` configurable):**

| Table | Purpose |
|-------|---------|
| `BC_Commit` | Commit header |
| `BC_Revision` | Per-entity revision row |
| `BC_Delta` | Property-level or JSON delta |
| `BC_Snapshot` | Full state snapshot |
| `BC_Comment` | User comments on revision/commit |
| `BC_Tag` | Tags (many-to-many with revision) |
| `BC_Metadata` | Key-value metadata |

Indexes designed for `(EntityType, EntityId, RevisionNumber DESC)` access pattern.

### Appendix C — Security Considerations

- Rollback is privileged operation — always authorize.
- PII redaction via `IChronicleCommitFilter` before persist.
- Optional field-level encryption via custom serializer.
- Audit log immutability: append-only tables; no public API to delete revisions (admin archival only).
- Multi-tenant: `TenantId` in `EntityRef` + row-level filtering in store.

### Appendix D — Comparison with Alternatives

| Capability | BusinessChronicle | Audit.NET | EF Core History | Event Sourcing |
|------------|-------------------|-----------|-----------------|----------------|
| Git-like model | ✅ | ❌ | ❌ | Partial |
| Rollback | ✅ | ❌ | ❌ | Replay |
| UI components | ✅ | ❌ | ❌ | Custom |
| EF integration | ✅ | ✅ | Manual | Custom |
| Learning curve | Low | Low | Medium | High |

### Appendix E — Open Design Questions (To Resolve During Implementation)

1. **Branching semantics** — git branches vs. draft/publish workflow for business users?
2. **Cross-entity commits** — single commit spanning Order + OrderLines (likely yes, matches SaveChanges batch).
3. **Soft delete chronicle** — how to represent deleted entities in timeline?
4. **Max delta size** — spill to blob storage threshold?
5. **MongoDB model** — document-per-revision vs. embedded array cap?

Track in GitHub Discussions before v2.0.

### Appendix F — Glossary

| Term | Definition |
|------|------------|
| **Chronicle** | Complete audit history for an entity |
| **Commit** | Atomic bundle of revisions from one operation |
| **Revision** | Single version state of one entity |
| **Snapshot** | Full serialized entity state at a point in time |
| **Delta** | Change set from parent revision |
| **Actor** | Identity that performed the change |
| **Rollback** | Restore entity to a prior revision (new forward revision) |

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 0.1.0-draft | 2026-07-01 | Architecture Team | Initial comprehensive design |

---

*This document is the authoritative architecture reference for BusinessChronicle. Implementation must align with this design; deviations require an ADR (Architecture Decision Record) in `docs/architecture/adr/`.*
