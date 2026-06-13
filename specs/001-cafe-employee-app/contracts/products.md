# API Contract: Products

**Base path**: `/api/products`
**Authentication**: Required (Entra ID via Azure SWA). All endpoints require `authenticated` role.

---

## GET /api/products

Returns all products. By default returns only active products; pass `?includeInactive=true`
to include deactivated products (any authenticated employee may call this).

**Query parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| includeInactive | bool | false | Include deactivated products in results |

**Response 200**:
```json
[
  {
    "id": 1,
    "name": "Croissant",
    "price": 3.50,
    "isActive": true,
    "createdAt": "2026-06-01T08:00:00Z"
  }
]
```

---

## GET /api/products/{id}

Returns a single product including its full price history.

**Response 200**:
```json
{
  "id": 1,
  "name": "Croissant",
  "price": 3.50,
  "isActive": true,
  "createdAt": "2026-06-01T08:00:00Z",
  "priceHistory": [
    { "price": 3.00, "effectiveFrom": "2026-06-01T08:00:00Z", "effectiveTo": "2026-06-10T09:15:00Z" },
    { "price": 3.50, "effectiveFrom": "2026-06-10T09:15:00Z", "effectiveTo": null }
  ]
}
```

**Response 404**: Product not found.

---

## POST /api/products

Creates a new product. Role: any authenticated employee.

**Request body**:
```json
{
  "name": "Cinnamon Roll",
  "price": 4.00
}
```

**Validation**:
- `name`: required, 1–100 characters, unique (case-insensitive)
- `price`: required, ≥ 0.01

**Response 201**: Returns the created product (same shape as GET /api/products/{id}).
**Response 400**: Validation error details.
**Response 409**: A product with this name already exists.

---

## PATCH /api/products/{id}

Updates a product's price and/or active status. Partial update — only supplied fields change.

**Request body** (all fields optional):
```json
{
  "price": 4.50,
  "isActive": false
}
```

**Validation**:
- `price`: if supplied, ≥ 0.01
- `isActive`: if supplied, boolean

**Response 200**: Returns the updated product.
**Response 404**: Product not found.
**Response 400**: Validation error details.
