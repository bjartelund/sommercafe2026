# Data Model: Café Employee App

**Date**: 2026-06-13
**Feature**: [plan.md](plan.md)

---

## Entities

### Employee

Represents a café staff member. Created on first login via Entra ID; the application does not
store passwords or manage credentials.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, identity | Internal surrogate key |
| EntraObjectId | nvarchar(36) | UNIQUE, NOT NULL | Entra ID user object ID (GUID) |
| DisplayName | nvarchar(100) | NOT NULL | Synced from Entra ID claims on login |
| Email | nvarchar(256) | NOT NULL | Synced from Entra ID claims on login |
| Role | nvarchar(20) | NOT NULL, default 'Staff' | Values: `Staff`, `Manager` |
| IsActive | bit | NOT NULL, default 1 | Soft-delete; inactive employees cannot log in |
| CreatedAt | datetime2 | NOT NULL | UTC timestamp of first login |

**Relationships**: One Employee → many Orders, Expenses, WorkSessions.

---

### Product *(temporal table)*

Represents an item available for sale. The table is system-versioned so every price change is
captured automatically in `ProductsHistory`.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, identity | |
| Name | nvarchar(100) | NOT NULL | Display name, e.g. "Croissant", "Flat White" |
| Price | decimal(10,2) | NOT NULL, ≥ 0 | Current selling price |
| IsActive | bit | NOT NULL, default 1 | False = deactivated; hidden from Order page |
| CreatedAt | datetime2 | NOT NULL | UTC, set on INSERT |
| SysStartTime | datetime2 | GENERATED ALWAYS AS ROW START | Managed by SQL Server |
| SysEndTime | datetime2 | GENERATED ALWAYS AS ROW END | Managed by SQL Server |

**PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime)**
**WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.ProductsHistory))**

**State transitions**:
- New → Active (`IsActive = 1`) on creation.
- Active → Deactivated (`IsActive = 0`) via explicit action; reversible.
- Price change → UPDATE `Price`; temporal table automatically records old row in history.

**Point-in-time price query** (used when displaying historical order details):
```sql
SELECT Price FROM Products FOR SYSTEM_TIME AS OF @OrderPlacedAt WHERE Id = @ProductId
```

---

### Order

A sales transaction created when an employee submits a customer's order.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, identity | |
| EmployeeId | int | FK → Employee.Id, NOT NULL | Who recorded the order |
| PlacedAt | datetime2 | NOT NULL | UTC timestamp of submission |
| TotalAmount | decimal(10,2) | NOT NULL, computed | Sum of all OrderLine.LineTotal values |
| Notes | nvarchar(500) | NULL | Optional free-text note |

**Relationships**: One Order → many OrderLines.

---

### OrderLine

A single product entry within an order. Stores a denormalised price snapshot to avoid
temporal joins on every read.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, identity | |
| OrderId | int | FK → Order.Id, NOT NULL | |
| ProductId | int | FK → Product.Id, NOT NULL | Reference to product (for name display) |
| ProductName | nvarchar(100) | NOT NULL | Snapshot of product name at time of order |
| UnitPrice | decimal(10,2) | NOT NULL | Snapshot of price at time of order |
| Quantity | int | NOT NULL, ≥ 1 | |
| LineTotal | decimal(10,2) | NOT NULL, computed | UnitPrice × Quantity |

**Note**: `ProductName` and `UnitPrice` are denormalised snapshots. The temporal table on
`Products` remains the authoritative historical record; `OrderLine` snapshots exist for
read performance.

---

### Expense

A business cost entry recorded by an employee.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, identity | |
| EmployeeId | int | FK → Employee.Id, NOT NULL | Who recorded the expense |
| Description | nvarchar(200) | NOT NULL | What was purchased/paid for |
| Amount | decimal(10,2) | NOT NULL, > 0 | Cost in local currency |
| ExpenseDate | date | NOT NULL | Date the expense occurred |
| CreatedAt | datetime2 | NOT NULL | UTC timestamp of data entry |
| UpdatedAt | datetime2 | NULL | UTC timestamp of last edit |

---

### WorkSession

A logged working period for one employee.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, identity | |
| EmployeeId | int | FK → Employee.Id, NOT NULL | |
| SessionDate | date | NOT NULL | Calendar date of the session |
| StartTime | time | NOT NULL | Local time of shift start |
| EndTime | time | NOT NULL | Local time of shift end; must be > StartTime |
| DurationMinutes | int | NOT NULL, computed | (EndTime - StartTime) in whole minutes |
| Notes | nvarchar(500) | NULL | Optional note (e.g. "covered for Alex") |
| CreatedAt | datetime2 | NOT NULL | UTC timestamp of entry |
| UpdatedAt | datetime2 | NULL | UTC timestamp of last edit |

**Validation**: `EndTime > StartTime` enforced at application layer; overnight shifts are
out of scope (assumption documented in spec).

---

## Entity Relationship Diagram (text)

```
Employee 1──* Order 1──* OrderLine *──1 Product (temporal)
Employee 1──* Expense
Employee 1──* WorkSession
```

---

## Database Notes

- All UTC timestamps stored as `datetime2`; display conversion to local time handled client-side.
- `ProductsHistory` table is managed automatically by SQL Server — no application migrations
  needed for the history table itself, only for the `Products` table.
- Connection string stored in Azure App Settings (production) and `local.settings.json`
  (local dev, git-ignored).
- EF Core migrations managed via `dotnet ef` in the `api/` project.
