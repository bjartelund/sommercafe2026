# Research: Café Employee App

**Date**: 2026-06-13
**Feature**: [plan.md](plan.md)

---

## 1. Azure SQL Temporal Tables for Price History

**Decision**: Use a system-versioned temporal table for the `Products` table.

**Rationale**: SQL Server temporal tables (ISO/IEC 9075 compliant, supported in Azure SQL)
automatically move superseded rows to a history table when an UPDATE occurs. This means every
price change is recorded without any application-layer audit logic. Querying the price in
effect at the moment an order was placed uses the standard `FOR SYSTEM_TIME AS OF @timestamp`
syntax, which is handled at the database layer with no extra join complexity in application code.

**How it works**:
- `Products` table has two hidden datetime2 columns: `SysStartTime`, `SysEndTime`.
- Azure SQL moves the old row to `ProductsHistory` automatically on every UPDATE.
- To retrieve the price at order time: `SELECT Price FROM Products FOR SYSTEM_TIME AS OF @OrderPlacedAt WHERE Id = @ProductId`.
- EF Core 6+ supports temporal tables via `modelBuilder.Entity<Product>().ToTable(t => t.IsTemporal())`.
- Deactivation is a logical flag (`IsActive = false`) on the current row — history is unaffected.

**Alternatives considered**:
- Custom `ProductPriceHistory` table: More application code, risk of missed writes, harder
  to query point-in-time state. Rejected in favour of temporal tables.
- Store price snapshot on OrderLine: Simpler query for order display but loses the ability
  to query "what was the price on date X" independently. Adopted as a complement (OrderLine
  stores `UnitPrice` at time of order as a denormalised snapshot for read performance), with
  temporal table as the authoritative source.

---

## 2. Microsoft Entra ID Authentication

**Decision**: Use Microsoft Entra ID (formerly Azure AD) with the MSAL library for Blazor WASM
and `Microsoft.Identity.Web` for Azure Functions token validation.

**Rationale**: Entra ID is the Azure-native identity platform. It integrates directly with
Azure Static Web Apps' built-in authentication, eliminating the need to manage a custom user
store. Employees sign in with their Microsoft work/school accounts. The `/.auth/me` endpoint
provided by Azure SWA exposes the authenticated user's claims to the Blazor app without custom
token-handling code.

**How it works — Blazor WASM**:
- `Microsoft.Authentication.WebAssembly.Msal` package wires MSAL into the Blazor DI container.
- `AuthorizeRouteView` in `App.razor` enforces authentication on all routes.
- `IAccessTokenProvider` provides tokens for attaching to API calls via `BaseAddressAuthorizationMessageHandler`.

**How it works — Azure Functions**:
- Azure SWA validates the Entra ID token before the request reaches Functions.
- Inside Functions, `Microsoft.Identity.Web` (`AddMicrosoftIdentityWebApiAuthentication`) reads
  the principal from the `x-ms-client-principal` header injected by SWA.
- Role checking (staff vs. manager) is done by reading a custom `role` claim stored in a local
  `Employees` table, looked up by Entra Object ID on first login.

**Alternatives considered**:
- Azure AD B2C: Designed for consumer identities, adds complexity for an employee-only app.
  Rejected.
- Custom JWT: No third-party identity provider, but requires managing secrets and user
  credentials. Rejected in favour of Entra ID.

---

## 3. EF Core with Azure SQL

**Decision**: Use EF Core 9 with the `Microsoft.EntityFrameworkCore.SqlServer` provider.

**Rationale**: EF Core is the standard ORM for .NET, supports Azure SQL temporal tables
natively (via `IsTemporal()`), and is well-supported in Azure Functions dotnet-isolated.
Code-first migrations via `dotnet ef migrations` manage schema evolution.

**Key configuration points**:
- `AppDbContext` registered in `api/Program.cs` with `UseSqlServer(connectionString)`.
- Connection string sourced from `local.settings.json` (local dev) or Azure App Settings
  (production) — never committed to source control.
- `Product` entity configured with `IsTemporal()` in `OnModelCreating`.
- `OrderLine` stores a denormalised `UnitPrice` (decimal) snapshot to avoid temporal joins
  on every order read.

---

## 4. Azure Static Web Apps Routing & Auth Integration

**Decision**: Use Azure SWA's built-in routing (`staticwebapp.config.json`) to protect all
routes and redirect unauthenticated users to the Entra ID login flow.

**Rationale**: SWA provides a zero-config authentication proxy. Defining `"allowedRoles":
["authenticated"]` on `"route": "/*"` ensures no route is reachable without a valid Entra ID
session, removing the need for per-endpoint auth guards in the Blazor app.

**staticwebapp.config.json skeleton**:
```json
{
  "routes": [
    {
      "route": "/*",
      "allowedRoles": ["authenticated"]
    }
  ],
  "responseOverrides": {
    "401": { "redirect": "/.auth/login/aad", "statusCode": 302 }
  },
  "auth": {
    "identityProviders": {
      "azureActiveDirectory": {
        "registration": {
          "openIdIssuer": "https://login.microsoftonline.com/<tenant-id>/v2.0",
          "clientIdSettingName": "AZURE_CLIENT_ID",
          "clientSecretSettingName": "AZURE_CLIENT_SECRET"
        }
      }
    }
  }
}
```

---

## 5. Mobile-Responsive UI Approach

**Decision**: Use Bootstrap 5 (already bundled in the Blazor WASM template) with a
mobile-first layout. No additional CSS framework required.

**Rationale**: Bootstrap 5 grid and utility classes cover the responsive requirements (single-
column on phones, wider on tablets/desktop). The existing `app.css` and Bootstrap bundle in
`wwwroot/lib/bootstrap` are sufficient. Custom components use flexbox/grid where Bootstrap
utilities fall short.

**Key practices**:
- All forms use `form-control` with `mb-3` spacing for touch-friendly tap targets.
- Navigation uses a collapsible sidebar/hamburger pattern (extend existing `NavMenu.razor`).
- Order page product picker uses large tap-target cards rather than a dropdown.
- Tables in Ledger/Work Hours use horizontal scroll on small screens via `table-responsive`.
