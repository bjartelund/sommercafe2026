<!--
SYNC IMPACT REPORT
==================
Version change: [TEMPLATE] → 1.0.0 (initial ratification)
Modified principles: N/A (all new)
Added sections:
  - Core Principles (I–V)
  - Technology Stack
  - Development Workflow
  - Governance
Removed sections: N/A
Templates reviewed:
  - .specify/templates/plan-template.md ✅ Constitution Check section aligns with principles below
  - .specify/templates/spec-template.md ✅ No principle-specific constraints to add
  - .specify/templates/tasks-template.md ✅ Observability task category (Phase N) aligns with Principle IV
Deferred TODOs: none
-->

# SummerCafe 2026 Constitution

## Core Principles

### I. Azure-Native Architecture

The application MUST be designed and deployed exclusively for the Azure platform.
Infrastructure choices MUST align with Azure Static Web Apps hosting constraints:
Blazor WebAssembly for the frontend, Azure Functions (dotnet isolated) for the API backend.
No local-only infrastructure, self-hosted services, or platform-agnostic abstractions
that undermine Azure-native capabilities (e.g., built-in auth, routing, CDN) are permitted.

### II. API-Driven Data Access

The Blazor client MUST NOT access data stores directly.
All data reads and writes MUST flow through Azure Functions API endpoints.
API contracts (routes, request/response shapes) MUST be defined before implementation begins.
This preserves security boundaries enforced by Azure Static Web Apps and enables
independent scaling of frontend and backend.

### III. Component-First Frontend

Blazor UI MUST be structured as self-contained, independently renderable components.
Pages MUST compose components; components MUST NOT embed page-level navigation logic.
Each component MUST have a single, clearly named responsibility.
Shared state that crosses component boundaries MUST use a dedicated service abstraction,
not component-to-component parameter threading.

### IV. Observability by Default

OpenTelemetry instrumentation MUST be wired up in the API from project start.
Structured logging is REQUIRED in all Azure Functions; unstructured string concatenation
in log calls is not permitted.
Azure Monitor exporter configuration MUST exist (even if commented out for local dev)
so that activating production telemetry requires only uncommenting, not adding new code.

### V. Simplicity First

Features MUST start as the simplest implementation that satisfies the acceptance criteria.
Abstractions are introduced only after a concrete need appears in at least two places.
Dependencies MUST be justified; adding a NuGet package requires a stated reason in the PR.
YAGNI applies: no speculative generality, no "we might need this later" extensions.

## Technology Stack

**Frontend**: Blazor WebAssembly on .NET 10.0 — `src/Client/`

**Backend**: Azure Functions dotnet-isolated on .NET 8.0 — `api/`

**Hosting**: Azure Static Web Apps (swa-cli for local development)

**Observability**: OpenTelemetry SDK + Azure Monitor Exporter (`api/Program.cs`)

**Tooling**: `dotnet watch run` for client hot-reload; `swa start` for full-stack local preview

No framework substitutions (e.g., replacing Blazor with React, or Functions with a generic
web server) are permitted without a formal constitution amendment.

## Development Workflow

Local development MUST use `swa-cli` (`swa start`) to replicate the Azure Static Web Apps
routing and authentication proxy. Running the Blazor client in isolation is acceptable only
for pure UI work; API-integrated features MUST be tested through the SWA local emulator.

Feature work MUST follow the speckit workflow: spec → plan → tasks → implement.
Each feature plan MUST include a Constitution Check gate verifying alignment with
Principles I–V before implementation begins.

Commits SHOULD be small and scoped to a single task. All API-breaking changes MUST be
discussed in the feature spec before implementation.

## Governance

This constitution supersedes all other development guidance documents within this project.
Amendments MUST:
1. Increment the version number (MAJOR for principle removal/redefinition,
   MINOR for new principles or sections, PATCH for clarifications).
2. Update `LAST_AMENDED_DATE` to the amendment date.
3. Run `/speckit-constitution` to propagate changes to dependent templates.

All feature plans and pull requests MUST include a Constitution Check confirming
compliance with active principles. Complexity that conflicts with Principle V (Simplicity)
MUST be explicitly justified with a concrete, current requirement — not a hypothetical one.

**Version**: 1.0.0 | **Ratified**: 2026-06-13 | **Last Amended**: 2026-06-13
