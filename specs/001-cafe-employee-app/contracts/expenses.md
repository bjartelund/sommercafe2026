# API Contract: Expenses

**Base path**: `/api/expenses`
**Authentication**: Required (Entra ID via Azure SWA).

---

## GET /api/expenses

Returns expenses. Supports date-range filtering.

**Query parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| from | date (ISO 8601) | 30 days ago | Inclusive start date (by ExpenseDate) |
| to | date (ISO 8601) | today | Inclusive end date |
| page | int | 1 | Page number (1-based) |
| pageSize | int | 50 | Results per page (max 200) |

**Response 200**:
```json
{
  "items": [
    {
      "id": 7,
      "description": "Flour and butter — Bakery Supply Co.",
      "amount": 45.80,
      "expenseDate": "2026-06-12",
      "employeeName": "Jonas Kvist",
      "createdAt": "2026-06-12T14:20:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 50
}
```

---

## POST /api/expenses

Records a new expense. The authenticated employee's ID is resolved from the Entra ID principal.

**Request body**:
```json
{
  "description": "Flour and butter — Bakery Supply Co.",
  "amount": 45.80,
  "expenseDate": "2026-06-12"
}
```

**Validation**:
- `description`: required, 1–200 characters
- `amount`: required, > 0
- `expenseDate`: required, ISO 8601 date, not in the future

**Response 201**: Returns the created expense (same shape as a single item in GET /api/expenses).
**Response 400**: Validation error details.

---

## PUT /api/expenses/{id}

Replaces all editable fields of an expense. Any authenticated employee may edit any expense
(no role restriction in current scope).

**Request body**:
```json
{
  "description": "Flour and butter — Bakery Supply Co. (corrected amount)",
  "amount": 47.20,
  "expenseDate": "2026-06-12"
}
```

**Validation**: Same as POST.

**Response 200**: Returns the updated expense.
**Response 404**: Expense not found.
**Response 400**: Validation error details.

---

## DELETE /api/expenses/{id}

Deletes an expense. Any authenticated employee may delete any expense (no role restriction
in current scope).

**Response 204**: Deleted successfully.
**Response 404**: Expense not found.
