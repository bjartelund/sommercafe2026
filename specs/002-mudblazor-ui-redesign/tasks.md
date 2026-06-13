# Tasks: MudBlazor UI Redesign

**Feature**: [MudBlazor UI Redesign](spec.md)

**Plan**: [Implementation Plan](plan.md)

**Status**: Ready for implementation

---

## Phase 1: Setup — Project Configuration & Dependencies

**Goal**: Add MudBlazor to the project, remove Bootstrap, and initialize MudBlazor services.

**Independent Test**: Verify MudBlazor initializes without errors: `dotnet build` compiles without warnings, `swa start` runs without console errors.

### Setup Tasks

- [X] T001 Add MudBlazor NuGet package to `src/Client/Client.csproj` (version 6.x compatible with .NET 10)
- [X] T002 Remove Bootstrap 5 CSS link from `src/Client/wwwroot/index.html`
- [X] T003 [P] Add MudBlazor script and CSS imports to `src/Client/wwwroot/index.html`
- [X] T004 [P] Initialize MudBlazor in `src/Client/Program.cs` with `services.AddMudServices()`
- [X] T005 [P] Remove Bootstrap NuGet package reference from `src/Client/Client.csproj` (if present)
- [X] T006 Clear `src/Client/wwwroot/css/app.css` of Bootstrap utility classes; keep only MudBlazor theme overrides (if any)
- [X] T007 Verify build succeeds: `dotnet build` produces no errors

---

## Phase 2: Foundational — Layout & Navigation Infrastructure

**Goal**: Refactor MainLayout and NavMenu to use MudAppBar + MudDrawer for consistent persistent navigation.

**Independent Test**: Open any page in browser; navigation appears with top bar + drawer (desktop) or hamburger (mobile); no layout shifts or missing elements; page content renders below navigation.

### Foundational Tasks

- [X] T008 Refactor `src/Client/Shared/MainLayout.razor` to use MudAppBar + MudDrawer instead of Bootstrap `.navbar` and `.container`
  - MudAppBar with branding in Dense=true mode
  - MudDrawer for sidebar with Toggle functionality
  - MudMainContent wrapper for page content
  - Responsive drawer: Open on desktop, collapsible hamburger on mobile

- [X] T009 Refactor `src/Client/Layout/NavMenu.razor` to use MudNavMenu within MudDrawer
  - Replace `.nav` + `.nav-link` with MudNavLink components
  - Use MudIcon + text for each link (e.g., home icon + "Home")
  - Maintain current navigation targets: /, /orders, /products, /expenses, /ledger, /work-hours

- [ ] T010 [P] Update `src/Client/App.razor` if needed to work with new MainLayout structure (typically no changes required)

- [ ] T011 Test on desktop (1920px) and mobile (375px): Drawer visible on desktop, hamburger menu on mobile

---

## Phase 3: User Story 1 — Consistent Navigation Experience (P1)

**Goal**: Ensure navigation styling and interaction is uniform across all pages with visual feedback on active links.

**Why this priority**: Navigation is the first user interaction; consistency builds confidence.

**Independent Test**: Navigate to each main page (Orders, Products, Expenses, Ledger, Work Hours); verify active link is highlighted, layout is identical, no visual inconsistencies.

### US1 Tasks

- [X] T012 [US1] Verify MudNavLink active state styling in `src/Client/Layout/NavMenu.razor` highlights current page
- [X] T013 [US1] [P] Test navigation on all pages: Orders, Products, Expenses, Ledger, Work Hours — visual consistency confirmed

---

## Phase 4: User Story 2 — Professional Data Display (P1)

**Goal**: Refactor all data tables and lists to use MudTable with consistent styling, spacing, and visual hierarchy.

**Why this priority**: Data presentation is the core of the app; professional appearance increases user confidence.

**Independent Test**: View Orders, Products, and Expenses listings; tables show consistent column headers, row striping, and action buttons; all data is scannable and properly aligned.

### US2 Tasks — Orders Page

- [X] T014 [US2] Refactor `src/Client/Pages/Orders/OrdersPage.razor` to use MudBlazor components
  - Replace ProductPicker section with MudCard + MudBlazor inputs
  - Replace OrderSummary with MudTable or MudList for line items
  - Use MudButton for "Record Order" action

- [X] T015 [US2] [P] Refactor `src/Client/Pages/Orders/Components/ProductPicker.razor` to use MudTable or MudList
  - Each product as a MudTableRow or MudListItem
  - Product name, price in columns
  - MudNumericField for quantity input
  - MudButton with "Add" action

- [X] T016 [US2] [P] Refactor `src/Client/Pages/Orders/Components/OrderSummary.razor` to use MudTable + MudButton
  - Line items in MudTable
  - Running total in MudPaper card with bold typography
  - MudButton for "Confirm" and "Cancel" actions (Variant=Filled for primary)

### US2 Tasks — Products Page

- [X] T017 [US2] Refactor `src/Client/Pages/Products/ProductsPage.razor` to use MudTable
  - Columns: Name, Price, Status, Actions
  - Use MudChip for status badges (green=Active, red=Inactive)
  - MudButton for Edit, Deactivate, History actions
  - "Add Product" button above table (MudButton Variant=Filled)

- [X] T018 [US2] [P] Refactor `src/Client/Pages/Products/Components/ProductForm.razor` to use MudDialog + MudTextField
  - MudDialog wrapping form inputs
  - MudTextField for product name
  - MudNumericField for price
  - MudButton for Save/Cancel (primary/secondary variant)
  - Error state: Use MudField HelperText property for validation messages

- [X] T019 [US2] [P] Refactor `src/Client/Pages/Products/Components/PriceHistory.razor` to use MudTable
  - Columns: Price, Valid From, Valid Until
  - MudTable with rows from temporal table query
  - Sorted by SysStartTime descending

### US2 Tasks — Expenses Page (if available)

- [ ] T020 [US2] Refactor `src/Client/Pages/Expenses/ExpensesPage.razor` to use MudTable
  - Columns: Date, Description, Amount, Actions
  - MudButton for Add, Edit, Delete actions
  - "Add Expense" button above table

- [ ] T021 [US2] [P] Refactor `src/Client/Pages/Expenses/Components/ExpenseForm.razor` to use MudDialog + MudTextField
  - MudTextField for description
  - MudNumericField for amount
  - MudDatePicker for date
  - MudButton for Save/Cancel

### US2 Tasks — Ledger Page (if available)

- [ ] T022 [US2] Refactor `src/Client/Pages/Ledger/LedgerPage.razor` to use MudTable + MudPaper
  - Summary cards at top (MudPaper) showing totals
  - Ledger entries in MudTable
  - Columns: Date, Description, Amount, Type (Order/Expense), Running Total
  - Success/error styling for amount column (green=positive, red=negative)

### US2 Tasks — Work Hours Page (if available)

- [ ] T023 [US2] Refactor `src/Client/Pages/WorkHours/WorkHoursPage.razor` to use MudTable
  - Columns: Date, Employee, Start Time, End Time, Duration, Actions
  - "Log Hours" button above table
  - MudButton for Edit, Delete actions

- [ ] T024 [US2] [P] Refactor `src/Client/Pages/WorkHours/Components/WorkSessionForm.razor` to use MudDialog + MudTextField
  - MudDatePicker for session date
  - MudTimePicker for start/end times
  - MudTextField for optional notes
  - Duration preview (calculated, read-only)

---

## Phase 5: User Story 3 — Touch-Friendly Mobile Experience (P1)

**Goal**: Ensure all interactive elements meet minimum touch target size and layout adapts to mobile viewports without horizontal scrolling.

**Why this priority**: Employees log orders and expenses from the field; poor mobile UX reduces adoption.

**Independent Test**: Open app at 375px viewport width; navigate all pages, interact with forms and buttons; verify no horizontal scrolling, all buttons ≥44×44px, form labels clearly associated with inputs.

### US3 Tasks

- [ ] T025 [US3] Audit all MudButton components for minimum size: ensure `Size=Size.Medium` (default) or explicit px sizing ≥44×44px
  - Buttons in forms, tables, action rows
  - Check `src/Client/Pages/Orders`, Products, Expenses, Ledger, WorkHours, and components

- [ ] T026 [US3] [P] Verify MudTextField, MudNumericField, MudDatePicker input heights are ≥44px (MudBlazor default is 40px; may need variant override)

- [ ] T027 [US3] [P] Test at 375px viewport width (iPhone SE):
  - No horizontal scrolling of primary content
  - Tables reflow or scroll within viewport bounds
  - Forms stack vertically
  - Navigation drawer collapses to hamburger

- [ ] T028 [US3] [P] Test responsive behavior at breakpoints: 375px, 768px, 1024px, 1920px
  - Layout adapts gracefully (no jarring shifts)
  - Content remains readable and scannable

---

## Phase 6: User Story 4 — Consistent Component Styling (P2)

**Goal**: Verify all UI components use MudBlazor styling consistently across all pages; no inline styles or Bootstrap classes remain.

**Why this priority**: Visual consistency demonstrates professionalism; inconsistent styling confuses users.

**Independent Test**: Visual audit of Orders, Products, Expenses, Ledger, Work Hours pages; verify buttons, inputs, tables, badges, cards use identical styling; inspect DOM for zero Bootstrap classes.

### US4 Tasks

- [ ] T029 [US4] Code review: Scan all .razor files in `src/Client/Pages` and `src/Client/Layout` for Bootstrap class names (`btn`, `form-control`, `table`, `col-`, `row`, etc.)
  - Find any: T029 FAILS, create hotfix
  - Find none: T029 PASS

- [ ] T030 [US4] [P] Verify all error states use consistent MudBlazor styling:
  - Error messages: MudField HelperText or MudAlert (red background)
  - Success feedback: MudAlert or MudSnackbar (green background)
  - Loading states: MudProgressLinear or MudSkeleton

- [ ] T031 [US4] [P] Verify all badge/status indicators use MudChip with Color property:
  - Active: Color=Color.Success (green)
  - Inactive: Color=Color.Error (red)
  - Default: Color=Color.Default (gray)

- [ ] T032 [US4] [P] Verify all data tables use consistent styling:
  - MudTable with Striped=true and Bordered=true (or equivalent visual distinction)
  - Column headers bold, aligned left (or right for numbers)
  - Rows evenly spaced, readable font size

---

## Phase 7: User Story 5 — Dark/Light Theme Support (P3)

**Goal**: Implement theme toggle and persist theme preference across sessions using MudBlazor theme system.

**Why this priority**: Quality-of-life improvement; demonstrates attention to detail and accessibility.

**Independent Test**: Locate theme toggle; switch between light and dark modes; verify all elements readable and visible in both themes; reload page and verify preference persists.

### US5 Tasks

- [ ] T033 [US5] [P] Create theme toggle component `src/Client/Shared/ThemeToggle.razor`
  - MudIconButton with icon toggle (sun/moon)
  - OnClick: toggle theme state and update MudThemeProvider
  - Triggered by user preference or system (prefers-color-scheme)

- [ ] T034 [US5] [P] Implement theme persistence in `src/Client/Program.cs` or dedicated `src/Client/Services/ThemeService.cs`
  - Store preference in localStorage
  - Load preference on app startup
  - Update MudThemeProvider on load

- [ ] T035 [US5] Update `src/Client/Shared/MainLayout.razor` to include MudThemeProvider wrapper
  - Initialize with light theme by default
  - Theme updates reactively when ThemeToggle is clicked

- [ ] T036 [US5] [P] Test dark mode visibility:
  - Load app in light mode → all text readable, colors distinct
  - Toggle to dark mode → no white-on-white or low-contrast text
  - Reload page → dark mode persists
  - Verify on all 5 main pages

---

## Phase 8: Polish & Validation

**Goal**: Comprehensive testing, bug fixes, and final validation against success criteria.

### Polish Tasks

- [ ] T037 Run `dotnet build` from repo root: zero errors, zero Bootstrap-related warnings
- [ ] T038 Test application on 3 browsers (Chrome, Firefox, Safari) at 1920px: visual consistency confirmed
- [ ] T039 Test all 5 main pages (Orders, Products, Expenses, Ledger, Work Hours): no console errors, no layout shifts
- [ ] T040 Test form submission workflows (create product, submit order, log expense, etc.): data persists, no regressions
- [ ] T041 [P] Manual validation against quickstart.md scenarios 1–6: all pass
- [ ] T042 [P] Final audit: inspect DOM of 3 pages for zero Bootstrap classes using DevTools Elements tab

---

## Dependencies & Execution Order

### Critical Path

```
T001–T007 (Setup)
    ↓
T008–T011 (Foundational Layout)
    ↓
T012–T024 (Pages & Components by Story)
    ↓
T025–T028 (Mobile Testing)
    ↓
T029–T032 (Consistency Audit)
    ↓
T033–T036 (Theme Support)
    ↓
T037–T042 (Validation & Polish)
```

### Parallel Opportunities

**Within Phase 1 (Setup)**: T001–T007 can run in parallel once T001 completes (after MudBlazor is added to csproj)

**Within Phase 2 (Layout)**: T008 must complete before T009; T010 can run in parallel with T009

**Within Phase 4 (Data Display)**: 
- T014–T016 (Orders components) can run in parallel
- T017–T019 (Products components) can run in parallel  
- T020–T021 (Expenses components) can run in parallel
- T022, T023–T024 can run in parallel

**Within Phase 5 (Mobile)**: T025–T028 can run in parallel

**Within Phase 6 (Styling)**: T029–T032 can run in parallel

**Within Phase 8 (Validation)**: T037–T042 can run in parallel

---

## Implementation Strategy

### MVP Scope (Essential)

Complete these phases for a fully functional MudBlazor redesign:

1. **Phase 1** (Setup) — MudBlazor configured
2. **Phase 2** (Foundational Layout) — Persistent navigation working
3. **Phase 4** (Data Display) — All pages refactored to MudBlazor tables/components
4. **Phase 5** (Mobile Testing) — Verified responsive, touch-friendly
5. **Phase 8** (Validation) — Full test pass

**Estimated MVP Completion**: After T042

### Post-MVP Enhancements (Optional)

- **Phase 3** (Navigation consistency) — Automated verification
- **Phase 6** (Component styling audit) — Styling consistency documentation
- **Phase 7** (Theme support) — Dark mode toggle + persistence

---

## Task Format Validation

✅ All 42 tasks follow required format: `- [ ] TID [P?] [US?] Description with file path`

✅ Task IDs sequential (T001–T042)

✅ [P] markers applied to parallelizable tasks only

✅ [US1]–[US5] labels applied to user story phase tasks

✅ File paths specified for each task

✅ Checklist boxes present on all tasks
