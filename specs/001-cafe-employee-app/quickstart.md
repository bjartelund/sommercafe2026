# Quickstart Validation Guide: Café Employee App

**Date**: 2026-06-13
**Feature**: [plan.md](plan.md)

This guide describes how to verify each user story works end-to-end after implementation.
It is not an implementation guide — see [data-model.md](data-model.md) and
[contracts/](contracts/) for schema and API details.

---

## Prerequisites

1. Azure SQL Database provisioned and connection string set in `api/local.settings.json`
   (key: `SqlConnectionString`).
2. Entra ID app registration created; `AZURE_CLIENT_ID` and tenant ID set in
   `api/local.settings.json` and `src/Client/wwwroot/appsettings.json`.
3. `staticwebapp.config.json` present at repo root with Entra ID auth configured
   (see [research.md](research.md) §4).
4. EF Core migrations applied: `dotnet ef database update` from `api/`.
5. SWA CLI installed: `npm install -g @azure/static-web-apps-cli`.

---

## Starting the App Locally

```bash
swa start
```

This starts the Blazor WASM dev server (port 8000) and Azure Functions (port 7071) together
behind the SWA proxy (port 4280). Open `http://localhost:4280` in a browser.

---

## Validation Scenarios

### Scenario 1 — Authentication Gate

**Goal**: Confirm no page is accessible without logging in.

1. Open `http://localhost:4280` in an incognito window.
2. **Expected**: Redirect to Microsoft login page (no café content visible).
3. Log in with a valid Entra ID account in the tenant.
4. **Expected**: Redirected to the café app home page.

---

### Scenario 2 — Record a Customer Order (US1 / P1)

**Goal**: Submit an order and confirm it appears in the Ledger.

1. Navigate to the **Products** page and create two products: "Espresso" (€2.50) and
   "Muffin" (€3.00).
2. Navigate to the **Orders** page.
3. Select 2× Espresso and 1× Muffin.
4. Submit the order.
5. **Expected**: Confirmation message; order visible on the page.
6. Navigate to the **Ledger**.
7. **Expected**: The new order appears with total €8.00 and today's date.

---

### Scenario 3 — Price History Preserved on Orders (US1 acceptance scenario 2)

**Goal**: Confirm a price change does not alter a previously recorded order.

1. (Continue from Scenario 2 — Espresso order at €2.50 exists in Ledger.)
2. Navigate to **Products** and change Espresso price to €3.00.
3. Navigate to **Ledger** and open the order from Scenario 2.
4. **Expected**: The order still shows Espresso at €2.50; total still €8.00.
5. Place a new order with 1× Espresso.
6. **Expected**: New order shows Espresso at €3.00.

---

### Scenario 4 — Deactivate a Product (US2)

**Goal**: Confirm a deactivated product disappears from the Order page.

1. Navigate to **Products** and deactivate "Muffin".
2. Navigate to **Orders**.
3. **Expected**: "Muffin" does not appear in the product list.
4. Return to **Products**.
5. **Expected**: "Muffin" is visible with a "Deactivated" status.

---

### Scenario 5 — Record and Edit an Expense (US3)

**Goal**: Record an expense and confirm it appears in the Ledger.

1. Navigate to **Expenses** and record: Description "Milk delivery", Amount €18.00,
   Date: today.
2. **Expected**: Expense appears in the list.
3. Navigate to **Ledger**.
4. **Expected**: Expense appears as a negative entry; net balance reflects both the order
   total and the expense.
5. Return to **Expenses**, edit the amount to €19.50, and save.
6. **Expected**: Ledger net balance updates accordingly.

---

### Scenario 6 — Date-Range Filter in Ledger (US4)

**Goal**: Confirm Ledger filtering works correctly.

1. Navigate to **Ledger** and set the date range to yesterday only.
2. **Expected**: No entries shown (all test data was created today).
3. Set the range to include today.
4. **Expected**: All orders and expenses from Scenarios 2–5 appear.

---

### Scenario 7 — Log Work Hours (US5)

**Goal**: Log a work session and confirm duration is calculated.

1. Navigate to **Work Hours** and log: Date today, Start 08:00, End 15:30.
2. **Expected**: Entry appears with Duration "7h 30m" (450 minutes).
3. Edit the entry: change End to 16:00.
4. **Expected**: Duration updates to "8h 00m" (480 minutes).

---

### Scenario 8 — Manager Sees All Employees' Hours (US5)

**Goal**: Confirm role-based access to work hours.

1. Log in as a Staff employee and log a work session.
2. Log out and log in as a Manager employee.
3. Navigate to **Work Hours**.
4. **Expected**: Manager can see work sessions for all employees, including the Staff
   employee's session from step 1.
5. Log out and log in as the original Staff employee.
6. Navigate to **Work Hours**.
7. **Expected**: Staff employee sees only their own sessions; no other employees' data visible.

---

## Pass Criteria Summary

| Scenario | Pass Condition |
|----------|---------------|
| 1 | Unauthenticated access redirects to login |
| 2 | Order total correct; appears in Ledger |
| 3 | Historical order shows original price after price change |
| 4 | Deactivated product absent from Order page; visible in Products |
| 5 | Expense appears in Ledger as negative; edits reflected |
| 6 | Date filter shows only matching entries |
| 7 | Duration calculated automatically; recalculates on edit |
| 8 | Manager sees all; Staff sees own only |
