---

description: "Task list for Café Employee App implementation"
---

# Tasks: Café Employee App

**Input**: Design documents from `specs/001-cafe-employee-app/`

**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/ ✅ quickstart.md ✅

**Tests**: TUnit integration tests (Testcontainers + Azure SQL) and unit tests per NFR-001/002.
Test tasks are included throughout.

**Organization**: Tasks are grouped by user story to enable independent implementation and
testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US5)
- All file paths are relative to repository root

---

## Phase 1: Setup

**Purpose**: Project initialization — packages, database, auth, test infrastructure

- [X] T001 Add `Microsoft.EntityFrameworkCore.SqlServer` and `Microsoft.EntityFrameworkCore.Tools` to `api/api.csproj`
- [X] T002 [P] Add `Microsoft.Identity.Web` to `api/api.csproj`
- [X] T003 [P] Add `Microsoft.Authentication.WebAssembly.Msal` to `src/Client/Client.csproj`
- [X] T004 Create test project `tests/CafeApp.Tests.csproj` targeting .NET 8 with TUnit, Testcontainers.MsSql, and Microsoft.EntityFrameworkCore.SqlServer packages
- [X] T005 [P] Create `api/Data/AppDbContext.cs` with empty `DbContext` subclass registered in `api/Program.cs` using `UseSqlServer` with connection string from `SqlConnectionString` app setting
- [X] T006 [P] Create `staticwebapp.config.json` at repo root configuring Entra ID auth, requiring `authenticated` role on `/*`, and redirecting 401 to `/.auth/login/aad`
- [X] T007 [P] Configure MSAL in `src/Client/Program.cs`: add `AddMsalAuthentication`, `AuthorizeRouteView` in `src/Client/App.razor`, and `BaseAddressAuthorizationMessageHandler` for API calls
- [X] T008 [P] Configure `Microsoft.Identity.Web` in `api/Program.cs`: add `AddMicrosoftIdentityWebApiAuthentication` and `AddAuthorization`; add `[Authorize]` to all future Function classes
- [X] T009 Create `tests/Infrastructure/DatabaseFixture.cs` with Testcontainers SQL Server container setup and `AppDbContext` scoped per test using TUnit `ClassDataSource`

---

## Phase 2: Foundational

**Purpose**: Shared entities, migrations, and Employee bootstrap — blocks all user stories

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T010 Create `api/Models/Employee.cs` with fields: `Id`, `EntraObjectId`, `DisplayName`, `Email`, `IsActive`, `CreatedAt` (no Role field — deferred per clarification)
- [X] T011 Add `Employee` `DbSet` to `api/Data/AppDbContext.cs` with unique index on `EntraObjectId`
- [X] T012 Create and apply initial EF Core migration for `Employee` table: `dotnet ef migrations add InitialEmployee` in `api/`
- [X] T013 Create `api/Functions/EmployeeFunctions.cs` with `POST /api/employees/me` — upserts the authenticated employee record from Entra ID claims (`x-ms-client-principal` header) on first login; returns the employee record
- [X] T014 Create `src/Client/Services/EmployeeService.cs` calling `POST /api/employees/me` on app startup to ensure the local employee record exists
- [X] T015 [P] Create `tests/Unit/` folder with `tests/Unit/ValidationTests.cs` — placeholder test class using TUnit `[Test]` attribute to verify the test project compiles and runs
- [X] T016 [P] Create `tests/Integration/` folder with `tests/Integration/EmployeeTests.cs` — integration test using `DatabaseFixture`: verify `POST /api/employees/me` creates an Employee row on first call and returns the same row on subsequent calls (upsert)

**Checkpoint**: Database running, auth wired, Employee record created on login — user story work can begin.

---

## Phase 3: User Story 1 — Record a Customer Order (P1) 🎯 MVP

**Goal**: Employee selects active products, submits an order; prices snapshot at submission time; order appears in the Ledger.

**Independent Test**: quickstart.md Scenarios 2 and 3 — submit an order, verify it appears in the Ledger, change a price, confirm old order still shows original price.

### Tests for User Story 1

- [X] T017 [P] [US1] Create `tests/Unit/OrderTotalTests.cs` — unit tests for order total calculation: sum of (UnitPrice × Quantity) across lines, including multi-line and single-line cases
- [X] T018 [P] [US1] Create `tests/Integration/ProductsTests.cs` — integration tests against Testcontainers DB: GET returns only active products; POST creates product; PATCH updates price and IsActive

### Implementation for User Story 1

- [X] T019 [P] [US1] Create `api/Models/Product.cs` with fields: `Id`, `Name`, `Price`, `IsActive`, `CreatedAt`, `SysStartTime`, `SysEndTime`
- [X] T020 [P] [US1] Create `api/Models/Order.cs` with fields: `Id`, `EmployeeId`, `PlacedAt`, `TotalAmount`, `Notes`; and `api/Models/OrderLine.cs` with fields: `Id`, `OrderId`, `ProductId`, `ProductName`, `UnitPrice`, `Quantity`, `LineTotal`
- [X] T021 Add `Product`, `Order`, `OrderLine` `DbSet`s to `api/Data/AppDbContext.cs`; configure `Product` with `IsTemporal()` in `OnModelCreating`; configure `Order`→`OrderLine` one-to-many relationship
- [X] T022 Create and apply EF Core migration for Product, Order, OrderLine tables: `dotnet ef migrations add AddProductsAndOrders` in `api/`
- [X] T023 [P] [US1] Create `api/Functions/ProductsFunctions.cs` implementing: `GET /api/products` (active only by default, `?includeInactive=true` optional), `GET /api/products/{id}` (with price history from temporal table), `POST /api/products` (validate name unique, price ≥ 0.01), `PATCH /api/products/{id}` (update price and/or IsActive)
- [X] T024 [US1] Create `api/Functions/OrdersFunctions.cs` implementing: `POST /api/orders` — resolves employee from claims, validates all lines reference active products, snapshots `ProductName` and `UnitPrice` from current Product row, computes `LineTotal` and `TotalAmount`, persists Order + OrderLines; `GET /api/orders` with date-range + pagination; `GET /api/orders/{id}` with full line detail
- [X] T025 [P] [US1] Create `src/Client/Services/ProductsService.cs` — HttpClient wrapper for `GET /api/products`, `GET /api/products/{id}`, `POST /api/products`, `PATCH /api/products/{id}`
- [X] T026 [P] [US1] Create `src/Client/Services/OrdersService.cs` — HttpClient wrapper for `POST /api/orders`, `GET /api/orders`, `GET /api/orders/{id}`
- [X] T027 [US1] Create `src/Client/Pages/Orders/OrdersPage.razor` composing `ProductPicker` and `OrderSummary` components; calls `ProductsService` on load; disables submit until ≥ 1 item selected; calls `OrdersService.SubmitOrder` on confirm
- [X] T028 [P] [US1] Create `src/Client/Pages/Orders/Components/ProductPicker.razor` — displays active products as large tap-target cards; emits quantity-changed events to parent; shows "No active products available" when list is empty
- [X] T029 [P] [US1] Create `src/Client/Pages/Orders/Components/OrderSummary.razor` — shows selected items, quantities, line totals, and running total; confirm/cancel actions
- [X] T030 [P] [US1] Add Orders link to `src/Client/Layout/NavMenu.razor`
- [X] T031 [US1] Create `tests/Integration/OrdersTests.cs` — integration tests: submit order snapshots current price; submit order after price change still shows original price on GET; submit with inactive product returns 400; submit with empty lines returns 400

**Checkpoint**: User Story 1 fully functional and testable independently. MVP deliverable.

---

## Phase 4: User Story 2 — Manage the Product Catalogue (P2)

**Goal**: Employee adds new products, changes prices, deactivates products; deactivated products disappear from Order page.

**Independent Test**: quickstart.md Scenario 4 — deactivate a product, confirm it's absent from Order page and visible with "Deactivated" status in Products page.

### Implementation for User Story 2

- [ ] T032 [P] [US2] Create `src/Client/Pages/Products/ProductsPage.razor` — lists all products (active + deactivated); "Add Product" button opens `ProductForm`; each row has Edit price and Deactivate/Reactivate actions; calls `ProductsService`
- [ ] T033 [P] [US2] Create `src/Client/Pages/Products/Components/ProductForm.razor` — inline or modal form for name + price; used for both add and edit; validates name non-empty and price > 0
- [ ] T034 [P] [US2] Create `src/Client/Pages/Products/Components/PriceHistory.razor` — expandable panel showing price history entries (effectiveFrom, effectiveTo, price) sourced from `GET /api/products/{id}`
- [ ] T035 [P] [US2] Add Products link to `src/Client/Layout/NavMenu.razor`
- [ ] T036 [US2] Create `tests/Integration/ProductsManagementTests.cs` — integration tests: add product appears active; change price — old orders unaffected, new orders use new price; deactivate — product absent from active list; price history returns all historical price rows

**Checkpoint**: User Stories 1 AND 2 independently functional and testable.

---

## Phase 5: User Story 3 — Record and Track Expenses (P3)

**Goal**: Employee records, edits, and hard-deletes expenses; expenses appear in the Ledger.

**Independent Test**: quickstart.md Scenario 5 — add an expense, confirm it appears in Expense list and in Ledger as a negative entry; edit amount, confirm Ledger updates.

### Tests for User Story 3

- [ ] T037 [P] [US3] Create `tests/Integration/ExpensesTests.cs` — integration tests: create expense appears in list; edit updates values; delete removes from list and Ledger; amount ≤ 0 returns 400

### Implementation for User Story 3

- [X] T038 [P] [US3] Create `api/Models/Expense.cs` with fields: `Id`, `EmployeeId`, `Description`, `Amount`, `ExpenseDate`, `CreatedAt`, `UpdatedAt`
- [X] T039 Add `Expense` `DbSet` to `api/Data/AppDbContext.cs`
- [X] T040 Create and apply EF Core migration for Expense table: `dotnet ef migrations add AddExpenses` in `api/`
- [X] T041 [P] [US3] Create `api/Functions/ExpensesFunctions.cs` implementing: `GET /api/expenses` (date-range + pagination), `POST /api/expenses` (validate description non-empty, amount > 0, date not future), `PUT /api/expenses/{id}`, `DELETE /api/expenses/{id}` (hard delete)
- [ ] T042 [P] [US3] Create `src/Client/Services/ExpensesService.cs` — HttpClient wrapper for all four expense endpoints
- [ ] T043 [US3] Create `src/Client/Pages/Expenses/ExpensesPage.razor` — lists expenses with date, description, amount; "Add Expense" button opens `ExpenseForm`; each row has Edit and Delete actions with confirmation prompt on delete
- [ ] T044 [P] [US3] Create `src/Client/Pages/Expenses/Components/ExpenseForm.razor` — form for description, amount, date; validates amount > 0 and date not in future
- [ ] T045 [P] [US3] Add Expenses link to `src/Client/Layout/NavMenu.razor`

**Checkpoint**: User Stories 1, 2, AND 3 independently functional and testable.

---

## Phase 6: User Story 4 — View the Financial Ledger (P3)

**Goal**: Employee views all orders and expenses in chronological order with net balance; supports date-range filtering; can drill into an order's line items.

**Independent Test**: quickstart.md Scenarios 5 and 6 — Ledger shows orders as positive and expenses as negative; date-range filter shows only matching entries.

### Tests for User Story 4

- [ ] T046 [P] [US4] Create `tests/Integration/LedgerTests.cs` — integration tests: Ledger returns orders as positive entries and expenses as negative; net balance correct; date-range filter excludes out-of-range entries; clicking an order entry returns full line detail

### Implementation for User Story 4

- [X] T047 [P] [US4] Create `api/Functions/LedgerFunctions.cs` implementing `GET /api/ledger` — unions orders (positive amounts) and expenses (negative amounts) filtered by date range, sorted by date descending, with summary totals (totalRevenue, totalExpenses, netBalance)
- [ ] T048 [P] [US4] Create `src/Client/Services/LedgerService.cs` — HttpClient wrapper for `GET /api/ledger`
- [ ] T049 [US4] Create `src/Client/Pages/Ledger/LedgerPage.razor` — date-range picker (defaults to current month); summary cards showing total revenue, total expenses, net balance; scrollable table of entries; tapping an Order entry navigates to order detail (reuses `OrdersService.GetOrderAsync`)
- [ ] T050 [P] [US4] Add Ledger link to `src/Client/Layout/NavMenu.razor`

**Checkpoint**: User Stories 1–4 all independently functional and testable.

---

## Phase 7: User Story 5 — Log and Review Work Hours (P4)

**Goal**: Employee logs work sessions (date, start time, end time); duration calculated automatically; all sessions visible to all authenticated users.

**Independent Test**: quickstart.md Scenarios 7 and 8 — log a session, verify duration; edit end time, verify duration recalculates; all sessions visible to any authenticated user.

### Tests for User Story 5

- [ ] T051 [P] [US5] Create `tests/Unit/WorkSessionDurationTests.cs` — unit tests for duration calculation: standard shift (08:00–16:00 = 480 min); minimum shift (1 min); endTime ≤ startTime returns validation error
- [ ] T052 [P] [US5] Create `tests/Integration/WorkSessionsTests.cs` — integration tests: create session calculates correct `DurationMinutes`; edit session recalculates duration; endTime before startTime returns 400; all sessions visible without filtering by employee

### Implementation for User Story 5

- [X] T053 [P] [US5] Create `api/Models/WorkSession.cs` with fields: `Id`, `EmployeeId`, `SessionDate`, `StartTime`, `EndTime`, `DurationMinutes`, `Notes`, `CreatedAt`, `UpdatedAt`
- [X] T054 Add `WorkSession` `DbSet` to `api/Data/AppDbContext.cs`
- [X] T055 Create and apply EF Core migration for WorkSession table: `dotnet ef migrations add AddWorkSessions` in `api/`
- [X] T056 [P] [US5] Create `api/Functions/WorkSessionsFunctions.cs` implementing: `GET /api/work-sessions` (date-range filter, optional employeeId filter — no role restriction), `POST /api/work-sessions` (validate endTime > startTime; compute DurationMinutes = (EndTime − StartTime).TotalMinutes), `PUT /api/work-sessions/{id}`, `DELETE /api/work-sessions/{id}`
- [ ] T057 [P] [US5] Create `src/Client/Services/WorkSessionsService.cs` — HttpClient wrapper for all four work-session endpoints
- [ ] T058 [US5] Create `src/Client/Pages/WorkHours/WorkHoursPage.razor` — date-range picker (defaults to current month); lists all sessions grouped by date showing employee name, start/end times, duration; "Log Hours" button opens `WorkSessionForm`; each row has Edit and Delete actions
- [ ] T059 [P] [US5] Create `src/Client/Pages/WorkHours/Components/WorkSessionForm.razor` — form for date, start time, end time, optional notes; validates endTime > startTime; previews calculated duration before save
- [ ] T060 [P] [US5] Add Work Hours link to `src/Client/Layout/NavMenu.razor`

**Checkpoint**: All five user stories independently functional and testable.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Observability, UX refinement, error handling, validation, documentation

- [ ] T061 [P] Add structured OpenTelemetry logging to all Azure Functions in `api/Functions/` — log operation name, employee ID, and entity ID on each mutating request (POST/PUT/PATCH/DELETE); no string concatenation in log calls
- [ ] T062 [P] Uncomment `UseAzureMonitorExporter()` in `api/Program.cs` and verify it activates cleanly when `APPLICATIONINSIGHTS_CONNECTION_STRING` app setting is present
- [ ] T063 [P] Add global error boundary component `src/Client/Shared/ErrorBoundary.razor` — catches unhandled exceptions and displays a user-friendly message with a retry option; wrap each page in `App.razor`
- [ ] T064 [P] Add loading skeleton or spinner to all data-fetching pages (`OrdersPage`, `ProductsPage`, `ExpensesPage`, `LedgerPage`, `WorkHoursPage`) while awaiting API responses
- [ ] T065 [P] Ensure all form inputs and buttons have minimum 44×44 px touch targets; audit `src/Client/Pages/` and `src/Client/Layout/` for horizontal overflow on 375px viewport
- [ ] T066 [P] Add `src/Client/wwwroot/appsettings.json` with `AzureAd` section (ClientId, Authority, DefaultScope) and document required values in a `docs/local-setup.md` file
- [ ] T067 [P] Run quickstart.md validation scenarios 1–8 manually against local SWA CLI environment; document any deviations in `specs/001-cafe-employee-app/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately; T001–T009 can all run in parallel
- **Foundational (Phase 2)**: Depends on Phase 1 completion — **BLOCKS all user stories**
- **US1 (Phase 3)**: Depends on Phase 2 — no other story dependencies
- **US2 (Phase 4)**: Depends on Phase 2; US1 must be complete (Products PATCH is exercised via Order page tests)
- **US3 (Phase 5)**: Depends on Phase 2; independent of US1 and US2
- **US4 (Phase 6)**: Depends on US1 (orders) and US3 (expenses) both complete
- **US5 (Phase 7)**: Depends on Phase 2; independent of US1–US4
- **Polish (Phase 8)**: Depends on all user stories complete

### User Story Dependencies

```
Phase 1 (Setup)
    └── Phase 2 (Foundational)
            ├── US1 (Orders + Products API + UI)
            │       └── US2 (Products Management UI)
            │               └── US4 (Ledger) ← also needs US3
            ├── US3 (Expenses) ─────────────────┘
            └── US5 (Work Hours) — fully independent
```

### Within Each User Story

- Integration test tasks marked [P] can start alongside model creation
- Models → DbContext → Migration → Functions → Client Service → Page → Components
- Commit after each task or logical group

### Parallel Opportunities

```bash
# Phase 1 — all parallel:
T001 T002 T003 T004 T005 T006 T007 T008 T009

# Phase 3 — parallel within story:
T017 T018 T019 T020  # tests + models simultaneously
T023 T024            # after migration (T022)
T025 T026 T027 T028 T029 T030  # client layer after API
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational — Employee record + auth
3. Complete Phase 3: US1 — Products API, Orders API, Order UI
4. **STOP AND VALIDATE**: Run quickstart.md Scenarios 1, 2, 3 on phone
5. Deploy to Azure SWA staging slot if ready

### Incremental Delivery

1. Phase 1 + 2 → Foundation ready
2. Phase 3 (US1) → Order taking works → **Demo / deploy**
3. Phase 4 (US2) → Full product management → **Demo / deploy**
4. Phase 5 (US3) → Expense tracking → **Demo / deploy**
5. Phase 6 (US4) → Full Ledger view → **Demo / deploy**
6. Phase 7 (US5) → Work hours → **Demo / deploy**
7. Phase 8 → Production-ready polish

---

## Notes

- `[P]` = different files, no unresolved dependencies — safe to run in parallel
- `[Story]` label maps task to its user story for traceability
- All test tasks use TUnit with `[Test]` attribute; integration tests use `DatabaseFixture` (Testcontainers)
- Role-based access control (staff vs. manager) is out of scope — all `[Authorize]` checks are authentication-only
- Hard delete on expenses — no soft-delete or audit trail
- Last-write-wins on concurrent price changes — no optimistic concurrency tokens needed
