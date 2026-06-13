# API Contract: Orders

**Base path**: `/api/orders`
**Authentication**: Required (Entra ID via Azure SWA).

---

## GET /api/orders

Returns orders. Supports date-range filtering.

**Query parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| from | date (ISO 8601) | 30 days ago | Inclusive start date |
| to | date (ISO 8601) | today | Inclusive end date |
| page | int | 1 | Page number (1-based) |
| pageSize | int | 50 | Results per page (max 200) |

**Response 200**:
```json
{
  "items": [
    {
      "id": 42,
      "placedAt": "2026-06-13T10:30:00Z",
      "employeeName": "Anna Berg",
      "totalAmount": 7.50,
      "lineCount": 2
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 50
}
```

---

## GET /api/orders/{id}

Returns full order details including all line items.

**Response 200**:
```json
{
  "id": 42,
  "placedAt": "2026-06-13T10:30:00Z",
  "employeeName": "Anna Berg",
  "totalAmount": 7.50,
  "notes": null,
  "lines": [
    {
      "productId": 1,
      "productName": "Croissant",
      "unitPrice": 3.50,
      "quantity": 1,
      "lineTotal": 3.50
    },
    {
      "productId": 3,
      "productName": "Flat White",
      "unitPrice": 4.00,
      "quantity": 1,
      "lineTotal": 4.00
    }
  ]
}
```

**Response 404**: Order not found.

---

## POST /api/orders

Submits a new order. The authenticated employee's ID is resolved from the Entra ID principal;
it is not supplied in the request body. Prices are resolved from the current product prices at
submission time and stored as snapshots on each OrderLine.

**Request body**:
```json
{
  "lines": [
    { "productId": 1, "quantity": 1 },
    { "productId": 3, "quantity": 2 }
  ],
  "notes": "Table 4"
}
```

**Validation**:
- `lines`: required, ≥ 1 item
- `lines[].productId`: must reference an active product
- `lines[].quantity`: integer ≥ 1
- `notes`: optional, max 500 characters

**Response 201**: Returns the created order (same shape as GET /api/orders/{id}).
**Response 400**: Validation error — e.g., empty lines array, inactive product referenced.
**Response 422**: One or more products not found.
