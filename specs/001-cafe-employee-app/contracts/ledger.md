# API Contract: Ledger

**Base path**: `/api/ledger`
**Authentication**: Required (Entra ID via Azure SWA).

The Ledger is a read-only derived view combining orders (revenue) and expenses (costs) into a
unified chronological financial record.

---

## GET /api/ledger

Returns ledger entries and summary totals for the requested date range.

**Query parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| from | date (ISO 8601) | start of current month | Inclusive start date |
| to | date (ISO 8601) | today | Inclusive end date |
| page | int | 1 | Page number (1-based) |
| pageSize | int | 100 | Results per page (max 500) |

**Response 200**:
```json
{
  "summary": {
    "totalRevenue": 420.00,
    "totalExpenses": 95.30,
    "netBalance": 324.70,
    "from": "2026-06-01",
    "to": "2026-06-13"
  },
  "items": [
    {
      "entryType": "Order",
      "referenceId": 42,
      "occurredAt": "2026-06-13T10:30:00Z",
      "description": "Order #42 (2 items)",
      "amount": 7.50,
      "employeeName": "Anna Berg"
    },
    {
      "entryType": "Expense",
      "referenceId": 7,
      "occurredAt": "2026-06-12T14:20:00Z",
      "description": "Flour and butter — Bakery Supply Co.",
      "amount": -45.80,
      "employeeName": "Jonas Kvist"
    }
  ],
  "totalCount": 2,
  "page": 1,
  "pageSize": 100
}
```

**Notes**:
- `amount` is positive for revenue (orders) and negative for expenses.
- `entryType` is either `"Order"` or `"Expense"`.
- `referenceId` links back to the originating Order or Expense record.
- Results are sorted by `occurredAt` descending (most recent first) by default.
