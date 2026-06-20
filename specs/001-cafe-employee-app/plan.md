# Implementation Plan: Café Employee App

**Branch**: `001-cafe-employee-app` | **Date**: 2026-06-13 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-cafe-employee-app/spec.md`

## Summary

A mobile-responsive employee web application for a small café. Employees can record customer
orders, manage the product catalogue, track expenses, view a financial ledger, and log work
hours. The application now runs as a standalone Blazor Server app backed by SQL Server, with
product price history preserved automatically using temporal tables.

## Technical Context

**Language/Version**: C# 13 / .NET 10

**Primary Dependencies**:
- `Microsoft.AspNetCore.Components` / Blazor Server — server-rendered interactive UI
- `Microsoft.EntityFrameworkCore.SqlServer` — EF Core + Azure SQL
- `MudBlazor` — component library for the UI

**Storage**: Azure SQL Database; Product table uses system-versioned temporal table for
automatic price history. All other tables are standard.

**Testing**: TUnit for both unit tests (pure logic) and integration tests (Testcontainers + Azure SQL). Blazor component tests (bunit) and E2E are out of scope.

**Target Platform**: Standalone ASP.NET Core / Blazor Server deployment; mobile browsers (iOS
Safari, Android Chrome) as primary client surface

**Project Type**: Employee-facing server-rendered interactive web app (Blazor Server)

**Performance Goals**: All pages fully interactive within 3 seconds on a 4G mobile connection;
order submission round-trip under 2 seconds

**Constraints**: Entra ID authentication required for every route; mobile-first responsive
layout; no offline operation required

**Scale/Scope**: ~10–20 employees, low transaction volume (tens of orders per day),
single-location café

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Azure-Native Architecture | ✅ PASS | ASP.NET Core Blazor Server app with SQL Server remains deployable on Azure App Service |
| II. API-Driven Data Access | ✅ PASS | Data access is centralized in injected server-side services; no client-side direct DB access |
| III. Component-First Frontend | ✅ PASS | Each page (Orders, Products, Expenses, Ledger, Work Hours) decomposes into focused components; shared state via injected services |
| IV. Observability by Default | ✅ PASS | OpenTelemetry already configured in api/Program.cs; structured logging required in all new Functions |
| V. Simplicity First | ✅ PASS | Temporal table handles price history without a custom audit log abstraction; no speculative categories or multi-location support |

**Pre-design gate: PASSED.** No violations to justify.

## Project Structure

### Documentation (this feature)

```text
specs/001-cafe-employee-app/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── products.md
│   ├── orders.md
│   ├── expenses.md
│   ├── ledger.md
│   └── work-sessions.md
└── tasks.md             # Phase 2 output (/speckit-tasks — NOT created here)
```

### Source Code (repository root)

```text
App/                                 # Standalone Blazor Server app
├── Components/
│   ├── Layout/
│   └── Pages/
│       ├── Orders/
│       ├── Products/
│       ├── Expenses/
│       ├── Ledger/
│       └── WorkHours/
├── Data/
│   └── AppDbContext.cs
├── Models/
│   ├── Product.cs
│   ├── Order.cs
│   ├── OrderLine.cs
│   ├── Expense.cs
│   ├── WorkSession.cs
│   └── Employee.cs
├── Services/                        # Server-side domain services per area
│   ├── ProductsService.cs
│   ├── OrdersService.cs
│   ├── ExpensesService.cs
│   ├── LedgerService.cs
│   └── WorkSessionsService.cs
└── Program.cs
```

**Structure Decision**: Consolidate the application into the existing `App` project so UI,
domain services, data access, and database models live in one standalone Blazor Server codebase.