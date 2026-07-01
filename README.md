<div align="center">

# BusinessChronicle

**Audit-grade change history for ERP, property management, and business systems.**

[![NuGet](https://img.shields.io/nuget/v/BusinessChronicle.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/BusinessChronicle/)
[![Build](https://img.shields.io/github/actions/workflow/status/your-org/BusinessChronicle/ci.yml?style=flat-square&logo=github)](https://github.com/your-org/BusinessChronicle/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg?style=flat-square)](LICENSE)
[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)

*Who changed it. What changed. When and why — with rollback, diffs, and compliance-ready timelines.*

[Quick start](#quick-start) · [Live samples](#live-samples) · [Architecture](#architecture) · [Packages](#packages) · [Docs](docs/ARCHITECTURE.md)

</div>

---

## Built for the systems you actually ship

If you build **ERP modules**, **commercial real estate platforms**, **property management**, or **line-of-business apps** for companies, you already know the pain:

| Without chronicle | With BusinessChronicle |
|---|---|
| `"UpdatedAt"` tells you *when*, not *what* | Structured field-level change records |
| Spreadsheet exports for auditors | Queryable audit trail per entity |
| "Who approved this lease?" → email archaeology | Actor, message, and correlation on every commit |
| Rollback = manual SQL scripts | Point-in-time revert through the API |
| Web, desktop, and mobile all diverge | One engine, many hosts |

BusinessChronicle brings **source-control semantics to business data** — commits, revisions, snapshots, diffs, tags, and timelines — so your ERP stays trustworthy as it scales.

---

## Live samples — Chronicle ERP (Property Portfolio)

All samples share one story: **Riverside Business Park — Unit 4B**, a commercial office unit moving through a real lease workflow.

```
Vacant  →  Listed  →  Under Offer  →  Lease Signed  →  Occupied
```

Each step writes a permanent audit entry — exactly what portfolio managers, finance, and compliance teams need.

### Run a demo in 30 seconds

**Web API + interactive UI (recommended for demos)**

```bash
cd samples/BusinessChronicle.Samples.MinimalApi
dotnet run
```

Open the browser → click through the lease workflow → watch the audit trail grow.

**MVC ERP portal**

```bash
cd samples/BusinessChronicle.Samples.Mvc
dotnet run
```

Navigate to **Properties** → open the unit → advance the lease pipeline.

**WPF desktop client**

```bash
cd samples/BusinessChronicle.Samples.Wpf
dotnet run
```

**WinForms desktop client**

```bash
cd samples/BusinessChronicle.Samples.WinForms
dotnet run
```

| Sample | Best for showing… |
|--------|-------------------|
| `BusinessChronicle.Samples.MinimalApi` | Modern API + Scalar docs + SPA-style demo page |
| `BusinessChronicle.Samples.Mvc` | Classic ERP web portal with portfolio views |
| `BusinessChronicle.Samples.Wpf` | Rich desktop experience for enterprise users |
| `BusinessChronicle.Samples.WinForms` | Legacy-friendly LOB desktop integration |
| `BusinessChronicle.Samples.Blazor` | Component-based web UI |
| `BusinessChronicle.Samples.React` | JavaScript frontends against the API |

Shared demo logic lives in `BusinessChronicle.Samples.Shared` — one domain model, consistent audit messages everywhere.

---

## Quick start

### Install

```bash
dotnet add package BusinessChronicle
dotnet add package BusinessChronicle.AspNetCore      # Web APIs
dotnet add package BusinessChronicle.EntityFrameworkCore.SqlServer  # SQL Server
```

### Register services

```csharp
builder.Services.AddBusinessChronicle(options =>
{
    options.Entity.AllowRollback = true;
});
```

### Map HTTP endpoints (Minimal API)

```csharp
app.MapBusinessChronicle();           // History, compare, rollback, timeline…
app.MapBusinessChronicleScalar();     // Scalar API docs (not Swagger UI)
```

### Record a business change

```csharp
ChronicleResult<IChronicleSession> sessionResult =
    await sessionFactory.CreateAsync(entityReference, cancellationToken);

await using IChronicleSession session = sessionResult.Value!;

await session.SaveRevisionAsync(
    entityReference,
    RevisionType.Update,
    changes,
    cancellationToken: cancellationToken);

session.CommitContext.Message = new CommitMessage
{
    Text = "Lease status updated after legal review.",
    ShortDescription = "Lease signed",
};

await session.CommitAsync(cancellationToken);
```

### Query history

```csharp
var history = await store.Query.GetEntriesAsync(
    entityReference,
    new TimelineQueryOptions { MaxResults = 50 },
    cancellationToken);
```

---

## Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│  Your application                                                    │
│  ERP · Property mgmt · Finance · CRM · Internal portals              │
└───────────────────────────────┬──────────────────────────────────────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        │                       │                       │
   AspNetCore              WPF / WinForms            Blazor / React
   Minimal APIs            Desktop LOB               Component UI
        │                       │                       │
        └───────────────────────┼───────────────────────┘
                                │
                    BusinessChronicle (meta)
                                │
              BusinessChronicle.EntityFrameworkCore
                     SqlServer · PostgreSQL · SQLite
                                │
                    BusinessChronicle.Core
                                │
                 BusinessChronicle.Abstractions
```

Full design: **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)**

---

## Packages

| Package | Purpose |
|---------|---------|
| **`BusinessChronicle`** | Meta package — start here |
| `BusinessChronicle.Abstractions` | Contracts, models, results |
| `BusinessChronicle.Core` | Engine, sessions, in-memory store |
| `BusinessChronicle.EntityFrameworkCore` | EF Core persistence & change capture |
| `BusinessChronicle.AspNetCore` | Minimal APIs, ProblemDetails, Scalar OpenAPI |
| `BusinessChronicle.Blazor` | Timeline, diff, comment components |
| `BusinessChronicle.Wpf` / `WinForms` | Desktop controls |
| `BusinessChronicle.React` | OpenAPI bridge for React apps |
| `BusinessChronicle.Cli` | `dotnet chronicle` global tool |
| `BusinessChronicle.Testing` | Fakes, assertions, snapshot helpers |
| `@business-chronicle/react` | TypeScript hooks & components |

**Database providers:** `EntityFrameworkCore.SqlServer` · `PostgreSql` · `Sqlite`

---

## Features

- **Immutable audit trail** — every commit is append-only with actor context
- **Entity timelines** — chronological views for property units, contracts, assets
- **Structural diffs** — compare any two revisions field-by-field
- **Rollback** — revert to a known-good revision with an explicit commit message
- **Metadata, tags, comments** — enrich entries for compliance workflows
- **Correlation IDs** — trace changes across services (ASP.NET Core integration)
- **EF Core interceptors** — capture `SaveChanges` without polluting domain code
- **AOT-ready core** — trim-friendly abstractions for high-performance services

---

## Ideal use cases

- **Commercial real estate** — lease lifecycle, unit status, tenant handovers
- **ERP modules** — purchase orders, inventory adjustments, approval chains
- **Property management** — maintenance records, occupancy history, rent revisions
- **Regulated industries** — demonstrable audit trails for internal and external review
- **Multi-channel apps** — same chronicle from web API, WPF desk, and mobile BFF

---

## Documentation

| Guide | Description |
|-------|-------------|
| [Architecture](docs/ARCHITECTURE.md) | Layering, boundaries, extension points |
| [Quick start](docs/guides/quick-start.md) | Step-by-step first integration |
| [Cookbook](docs/cookbook/common-scenarios.md) | Real-world patterns |
| [FAQ](docs/faq.md) | Common questions |
| [API reference](docs/api/README.md) | Generated API docs |
| [Contributing](CONTRIBUTING.md) | How to contribute |

---

## Development

```bash
git clone https://github.com/your-org/BusinessChronicle.git
cd BusinessChronicle
dotnet build BusinessChronicle.slnx
dotnet test
```

---

## Roadmap

| Milestone | Status |
|-----------|--------|
| Core engine & abstractions | ✅ |
| EF Core + SQL providers | ✅ |
| ASP.NET Core + Scalar | ✅ |
| Blazor, WPF, WinForms, React | ✅ |
| CLI & testing package | ✅ |
| Source generators & analyzers | 🔜 Future release |

See [CHANGELOG.md](CHANGELOG.md) for release notes.

---

## Contributing

We welcome issues and pull requests. Please read [CONTRIBUTING.md](CONTRIBUTING.md) and our [Code of Conduct](CODE_OF_CONDUCT.md).

- [Security policy](SECURITY.md)
- [Support](SUPPORT.md)

---

## License

MIT — see [LICENSE](LICENSE).

---

<div align="center">

**BusinessChronicle** — *trust your data's history as much as you trust your data.*

</div>
