# Implementation Plan: MudBlazor UI Redesign

**Branch**: `002-mudblazor-ui-redesign` | **Date**: 2026-06-13 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-mudblazor-ui-redesign/spec.md`

## Summary

Replace the default Blazor Bootstrap-based UI with MudBlazor components across all client pages and components. This UI-layer-only refactoring maintains all existing functionality and API contracts while improving visual consistency, accessibility, and mobile responsiveness. The implementation preserves the component hierarchy and business logic; only presentation implementation changes.

## Technical Context

**Language/Version**: C# 13 / .NET 10 Blazor WebAssembly

**Primary Dependencies**: MudBlazor v6.x (replaces Bootstrap 5)

**Storage**: N/A (UI layer only; no data schema changes)

**Testing**: Manual visual testing; existing integration tests remain unchanged

**Target Platform**: Blazor WebAssembly (browser-based, responsive to 375px–1920px viewports)

**Project Type**: Single-Page Application (SPA) / Web Service Frontend

**Performance Goals**: No degradation; maintain current load times and runtime performance

**Constraints**: Zero Bootstrap usage; all styling from MudBlazor theme system

**Scale/Scope**: 5 main pages (Orders, Products, Expenses, Ledger, Work Hours) + shared components (NavMenu, Layout, Forms)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

✅ **Principle III (Component-First Frontend)**: PASS
- UI refactoring maintains component boundaries and single-responsibility pattern
- No changes to component public interfaces (parameters, EventCallbacks)
- Shared navigation pattern (persistent sidebar + responsive icon rail) reinforces component composition

✅ **Principle V (Simplicity First)**: PASS
- MudBlazor is a direct replacement for Bootstrap; no new abstractions introduced
- One-to-one mapping of Bootstrap components to MudBlazor equivalents
- No dependency chains added; MudBlazor is self-contained

✅ **All Other Principles**: NOT AFFECTED
- Principle I (Azure-Native): UI layer unchanged; no infrastructure impact
- Principle II (API-Driven): No API changes; data access flow unchanged
- Principle IV (Observability): No logging changes; client-side telemetry unaffected

## Project Structure

### Documentation (this feature)

```text
specs/002-mudblazor-ui-redesign/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command) [minimal—UI-only]
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command) [N/A—UI-only]
└── tasks.md             # Phase 2 output (/speckit-tasks command)
```

### Source Code (Blazor WebAssembly Client)

```text
src/Client/
├── Pages/
│   ├── Orders/
│   │   ├── OrdersPage.razor          [REFACTOR to MudBlazor]
│   │   └── Components/
│   │       ├── ProductPicker.razor   [REFACTOR to MudBlazor]
│   │       └── OrderSummary.razor    [REFACTOR to MudBlazor]
│   ├── Products/
│   │   ├── ProductsPage.razor        [REFACTOR to MudBlazor]
│   │   └── Components/
│   │       ├── ProductForm.razor     [REFACTOR to MudBlazor]
│   │       └── PriceHistory.razor    [REFACTOR to MudBlazor]
│   ├── Expenses/
│   │   ├── ExpensesPage.razor        [REFACTOR to MudBlazor]
│   │   └── Components/
│   │       └── ExpenseForm.razor     [REFACTOR to MudBlazor]
│   ├── Ledger/
│   │   └── LedgerPage.razor          [REFACTOR to MudBlazor]
│   └── WorkHours/
│       ├── WorkHoursPage.razor       [REFACTOR to MudBlazor]
│       └── Components/
│           └── WorkSessionForm.razor [REFACTOR to MudBlazor]
├── Layout/
│   ├── MainLayout.razor              [REFACTOR to use responsive sidebar layout]
│   └── NavMenu.razor                 [REFACTOR to icon-first MudNavMenu]
├── Shared/
│   └── App.razor                     [No changes; wraps layout and router]
├── wwwroot/
│   ├── index.html                    [Remove Bootstrap link; add MudBlazor theme]
│   └── css/
│       └── app.css                   [Minimal custom CSS only; theme-driven]
└── Client.csproj                     [Add MudBlazor NuGet; remove Bootstrap ref]
```

**Structure Decision**: UI-layer-only refactoring preserves existing code structure. MudBlazor components replace Bootstrap HTML elements and utility classes one-to-one. Shared layout strategy (persistent sidebar with responsive icon rail behavior) applied consistently across all pages.

## Complexity Tracking

No complexity violations. This is a straightforward UI replacement that simplifies the codebase by:
- Removing Bootstrap HTML classes and utility CSS (currently scattered across .razor files)
- Adopting MudBlazor's theme system (replaces custom color/spacing overrides)
- Reducing custom CSS file to minimal theme tweaks