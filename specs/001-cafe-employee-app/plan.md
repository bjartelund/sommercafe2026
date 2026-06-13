# Implementation Plan: Café Employee App

**Branch**: `001-cafe-employee-app` | **Date**: 2026-06-13 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-cafe-employee-app/spec.md`

## Summary

A mobile-responsive, employee-only web application for a small café. Employees log in via
Microsoft Entra ID and can record customer orders, manage the product catalogue, track expenses,
view a financial ledger, and log work hours. The backend is Azure Functions (dotnet isolated)
backed by Azure SQL; the frontend is Blazor WebAssembly hosted on Azure Static Web Apps.
Product price history is preserved automatically using Azure SQL temporal tables.

## Technical Context

**Language/Version**: C# 13 / .NET 10 (Blazor WASM client); C# 12 / .NET 8 (Azure Functions API)

**Primary Dependencies**:
- `Microsoft.AspNetCore.Components.WebAssembly` 10.x — Blazor WASM host
- `Microsoft.Authentication.WebAssembly.Msal` — MSAL/Entra ID auth in browser
- `Microsoft.Identity.Web` — token validation in Azure Functions
- `Microsoft.EntityFrameworkCore.SqlServer` — EF Core + Azure SQL
- `Microsoft.Azure.Functions.Worker` — Functions host (already in project)
- `OpenTelemetry.*` — observability (already wired in api/Program.cs)

**Storage**: Azure SQL Database; Product table uses system-versioned temporal table for
automatic price history. All other tables are standard.

**Testing**: TUnit for both unit tests (pure logic) and integration tests (Testcontainers + Azure SQL). Blazor component tests (bunit) and E2E are out of scope.

**Target Platform**: Azure Static Web Apps (WASM + Functions); mobile browsers (iOS Safari,
Android Chrome) as primary client surface

**Project Type**: Employee-facing SPA (Blazor WASM) with REST API backend (Azure Functions)

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
| I. Azure-Native Architecture | ✅ PASS | Blazor WASM on Azure SWA, Azure Functions backend, Azure SQL, Entra ID — all Azure-native |
| II. API-Driven Data Access | ✅ PASS | Blazor client communicates only via Azure Functions HTTP endpoints; no direct DB access from WASM |
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
src/Client/                          # Blazor WebAssembly frontend (existing)
├── Pages/
│   ├── Orders/
│   │   ├── OrdersPage.razor         # Order submission page
│   │   └── Components/
│   │       ├── ProductPicker.razor
│   │       └── OrderSummary.razor
│   ├── Products/
│   │   ├── ProductsPage.razor
│   │   └── Components/
│   │       ├── ProductForm.razor
│   │       └── PriceHistory.razor
│   ├── Expenses/
│   │   ├── ExpensesPage.razor
│   │   └── Components/
│   │       └── ExpenseForm.razor
│   ├── Ledger/
│   │   └── LedgerPage.razor
│   └── WorkHours/
│       ├── WorkHoursPage.razor
│       └── Components/
│           └── WorkSessionForm.razor
├── Services/                        # HttpClient wrappers per domain
│   ├── ProductsService.cs
│   ├── OrdersService.cs
│   ├── ExpensesService.cs
│   ├── LedgerService.cs
│   └── WorkSessionsService.cs
└── Layout/                          # Existing — extend NavMenu

api/                                 # Azure Functions backend (existing)
├── Functions/
│   ├── ProductsFunctions.cs
│   ├── OrdersFunctions.cs
│   ├── ExpensesFunctions.cs
│   ├── LedgerFunctions.cs
│   └── WorkSessionsFunctions.cs
├── Data/
│   ├── AppDbContext.cs
│   └── Migrations/
├── Models/
│   ├── Product.cs
│   ├── Order.cs
│   ├── OrderLine.cs
│   ├── Expense.cs
│   ├── WorkSession.cs
│   └── Employee.cs
└── Program.cs                       # Existing — add EF Core + Identity.Web registration
```

**Structure Decision**: Existing `src/Client` (Blazor WASM) and `api` (Azure Functions) layout
retained. Pages are added under `src/Client/Pages/` using sub-folders per domain; Functions are
added under `api/Functions/` one file per domain. No new projects required.
