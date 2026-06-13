# Feature Specification: Café Employee App

**Feature Branch**: `001-cafe-employee-app`

**Created**: 2026-06-13

**Status**: Draft

**Input**: User description: "The application will support a small cafe offering an assorted selection of baked goods and drinks. There will be a ledger, an expense tracker, products page, an order page. the products page allows addition of new products, deactivation of products and change of prices (although old pricing should be available). The ledger keeps tracks of all orders and expenses. there is also a work hours reporting page. Only employee facing. the frontend should be modern looking and responsive so that employee may use phone."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Record a Customer Order (Priority: P1)

An employee takes a customer's order at the counter or tableside using their phone or a shared
screen. They select items from the active product catalogue, confirm quantities, and submit the
order. The order is saved and appears in the ledger immediately.

**Why this priority**: Core revenue-generating action of the café. Every other module depends
on orders being recorded correctly.

**Independent Test**: Launch the app on a phone browser, navigate to the Order page, add two
products, submit the order, then open the Ledger and confirm the order appears with the correct
line items and total.

**Acceptance Scenarios**:

1. **Given** the employee is on the Order page and the catalogue has active products, **When**
   they select one or more items and confirm the order, **Then** the order is saved with a
   timestamp, the items selected, and the prices current at time of order.
2. **Given** a product's price was changed since a previous order, **When** the employee views
   the old order in the Ledger, **Then** the old order still shows the price that was in effect
   when the order was placed.
3. **Given** there are no active products, **When** the employee opens the Order page, **Then**
   they see a clear message that no products are available and cannot submit an empty order.

---

### User Story 2 - Manage the Product Catalogue (Priority: P2)

An employee (or manager) maintains the list of items available for ordering: adding new products,
updating prices, and deactivating items that are no longer sold.

**Why this priority**: Without an accurate catalogue, order-taking is impossible. Keeping the
catalogue current is a daily operational task.

**Independent Test**: On the Products page, add a new product, save it, then change its price,
save again, then deactivate it. Verify the product no longer appears in the Order page's active
list, but its history is still visible in the Products page.

**Acceptance Scenarios**:

1. **Given** the employee is on the Products page, **When** they fill in a name and price for a
   new product and save, **Then** the product appears as active and is immediately available on
   the Order page.
2. **Given** an existing product, **When** the employee changes its price and saves, **Then** the
   new price is used for all future orders while existing orders retain the price at time of
   placement.
3. **Given** an active product, **When** the employee deactivates it, **Then** it no longer
   appears as selectable on the Order page but remains visible in the Products page with a
   "Deactivated" status.
4. **Given** a deactivated product, **When** the employee views its price history, **Then** all
   previous price changes are listed with the date each price became effective.

---

### User Story 3 - Record and Track Expenses (Priority: P3)

An employee records a business expense (e.g., ingredient purchase, supply run) with a
description, amount, and date. All expenses are visible in the Expense Tracker and also roll up
into the Ledger.

**Why this priority**: Expense tracking is essential for financial oversight but does not block
order operations. It can be added after core ordering is in place.

**Independent Test**: Navigate to the Expense Tracker, add an expense with a description and
amount, save it, and confirm it appears in the Expense Tracker list and is reflected in the
Ledger's expense totals.

**Acceptance Scenarios**:

1. **Given** the employee is on the Expense Tracker page, **When** they enter a description,
   amount, and date then save, **Then** the expense appears in the list with all entered details.
2. **Given** one or more expenses exist, **When** the employee opens the Ledger, **Then** total
   expenses for the current period are shown alongside total revenue from orders.
3. **Given** an expense was entered incorrectly, **When** the employee edits the expense,
   **Then** the updated values are saved and the Ledger reflects the correction.

---

### User Story 4 - View the Financial Ledger (Priority: P3)

An employee or manager reviews the café's financial activity: all orders taken and all expenses
recorded, summarised by day or period.

**Why this priority**: Read-only reporting feature; depends on orders and expenses being
recorded first.

**Independent Test**: After recording at least one order and one expense, open the Ledger and
verify both appear, with correct totals for revenue, expenses, and net balance.

**Acceptance Scenarios**:

1. **Given** orders and expenses exist, **When** the employee opens the Ledger, **Then** they
   see a list of all entries (orders and expenses) with dates, descriptions, and amounts, plus
   a running net balance.
2. **Given** the Ledger is open, **When** the employee filters by date range, **Then** only
   entries within that range are shown and totals update accordingly.
3. **Given** a historical order is shown in the Ledger, **When** the employee selects it,
   **Then** they can see the full order details including each line item and the price paid.

---

### User Story 5 - Log and Review Work Hours (Priority: P4)

An employee logs their working hours (start time, end time, date) and can review their own
logged hours. A manager view shows hours for all employees.

**Why this priority**: Supplementary HR function; independent of financial operations.

**Independent Test**: Log a work session with a start and end time, save it, then view the
Work Hours page and confirm the entry appears with the correct duration calculated.

**Acceptance Scenarios**:

1. **Given** the employee is on the Work Hours page, **When** they enter a date, start time,
   and end time and save, **Then** the entry appears in their hours list with the calculated
   duration.
2. **Given** multiple work sessions exist, **When** any authenticated employee views the Work
   Hours page, **Then** all sessions are visible (no role restriction in current scope).
3. **Given** an employee's hours entry contains an error, **When** they edit and save it,
   **Then** the corrected values and recalculated duration are shown.

---

### Edge Cases

- Submitting an order with zero items selected is prevented by validation (FR-001 requires
  one or more active products); the submit action is disabled until at least one item is added.
- If a price changes while an order is being assembled but not yet submitted, the server uses
  the price current at submission time (last-write-wins; no client warning required).
- An expense amount of zero or a negative number is rejected by validation; the amount field
  MUST be greater than zero.
- If end time is earlier than or equal to start time, the entry is rejected by validation.
  Overnight shifts are out of scope (assumption documented in Assumptions).
- If a product is deactivated while it is on an in-progress (not yet submitted) order, the
  submission will fail validation since FR-001 requires active products at time of submission
  (consistent with last-write-wins policy).

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Employees MUST be able to create an order by selecting one or more active products
  and submitting it; each order MUST record the exact price of each item at the time of
  submission.
- **FR-002**: Employees MUST be able to add new products with a name and price.
- **FR-003**: Employees MUST be able to change a product's price; previous prices MUST be
  retained and associated with orders placed under those prices.
- **FR-004**: Employees MUST be able to deactivate a product so it no longer appears on the
  Order page; deactivated products MUST remain visible in the Products page.
- **FR-005**: Employees MUST be able to record an expense with a description, amount, and date.
- **FR-006**: Employees MUST be able to edit or permanently delete a previously recorded
  expense. Deletion is hard delete — the record is removed and will no longer appear in the
  Ledger.
- **FR-007**: The Ledger MUST display all orders and all expenses in chronological order with
  a net balance.
- **FR-008**: The Ledger MUST support filtering by date range.
- **FR-009**: Employees MUST be able to log a work session with a date, start time, and end
  time; the system MUST calculate and display the duration automatically.
- **FR-010**: All authenticated employees MUST be able to view all logged work hours (no
  role restriction in current scope; manager-only filtering is deferred to a future version).
- **FR-011**: All pages MUST be fully usable on a mobile phone screen without horizontal
  scrolling or inaccessible controls.
- **FR-012**: Each employee MUST log in with individual credentials. Orders, expenses, and work
  hours MUST be attributed to the employee who created them. Manager role is a per-account
  setting that grants access to all employees' work hours and any manager-only views.

### Key Entities

- **Product**: An item offered for sale. Has a name, active/inactive status, and a price
  history (each price has an effective-from date).
- **Order**: A transaction recording one or more product line items sold at a specific time.
  Each line item captures the product and the price in effect at that time.
- **Expense**: A business cost entry with a description, amount, and date.
- **Ledger Entry**: A read-only view entry derived from orders and expenses, providing a
  chronological financial record.
- **Work Session**: A record of an employee's working period, capturing date, start time,
  end time, and calculated duration.
- **Employee**: A user of the system. Has a name and a role (staff or manager).

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Employees can record a complete customer order in under 60 seconds from opening
  the Order page to submission.
- **SC-002**: The Ledger accurately reflects 100% of submitted orders and recorded expenses
  with no missing or duplicated entries.
- **SC-003**: Price changes to a product do not alter the amounts shown on any previously
  submitted order — verifiable by placing an order, changing the price, and comparing ledger
  entries before and after.
- **SC-004**: All pages load and are fully interactive within 3 seconds on a standard mobile
  network connection.
- **SC-005**: Employees can complete the work hours logging task in under 2 minutes with no
  training beyond a brief verbal instruction.
- **SC-006**: 100% of active products appear on the Order page; 0% of deactivated products
  appear on the Order page.

---

## Assumptions

- All users of the application are café employees; there is no public-facing or customer-facing
  interface.
- Within the current scope there is a single employee who serves as both staff and manager.
  All authenticated users have identical access to all features. Role-based access control
  (staff vs. manager distinction) is out of scope for this version.
- The café operates with a single currency; no multi-currency support is required.
- Products are categorised implicitly by type (baked goods vs. drinks) but the spec assumes
  a flat list is sufficient for a small café; categories can be added later.
- Expenses are recorded manually by employees; there is no automatic import from bank feeds
  or receipt scanning in this version.
- Work hours logging is manual (enter start/end time); clock-in/clock-out functionality is
  out of scope for this version.
- The application operates in a single timezone (the café's local timezone).
- Internet connectivity is assumed to be available at the point of order and data entry;
  full offline operation is out of scope.
- Each employee has an individual account. Authentication is required to use any part of
  the application. There is no anonymous or shared-account access.

---

## Clarifications

### Session 2026-06-13

- Q: What test layers are required? → A: Integration tests (Testcontainers + Azure SQL, TUnit framework) plus unit tests for pure logic (duration calculation, order totals, validation rules). Blazor component tests and E2E are out of scope for this version.
- Q: Who can manage the product catalogue and access manager-level views? → A: Within current scope there is only one employee who serves as both staff and manager. All authenticated users have full access to all features. Role-based restrictions (staff vs. manager) are deferred to a future version.
- Q: How should a price change made while an order is being assembled be handled? → A: Last-write-wins — server uses the price current at submission time; no client-side staleness warning or refresh required.
- Q: Should expense deletion be hard or soft delete? → A: Hard delete — expense is permanently removed and disappears from the Ledger immediately. No audit trail retention required.

### Non-Functional Requirements

- **NFR-001**: The test suite MUST include integration tests powered by Testcontainers running
  an Azure SQL container, using TUnit as the test framework.
- **NFR-002**: Unit tests MUST cover pure business logic: work session duration calculation,
  order total computation, and input validation rules (zero/negative amounts, empty order lines,
  end-time-before-start-time). No database or HTTP dependencies in unit tests.
- **NFR-003**: Blazor component tests (bunit) and end-to-end browser tests are explicitly out
  of scope for this version.
