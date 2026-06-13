# API Contract: Work Sessions

**Base path**: `/api/work-sessions`
**Authentication**: Required (Entra ID via Azure SWA).

---

## GET /api/work-sessions

Returns work sessions. Staff employees see only their own sessions. Managers see sessions for
all employees and may filter by employee.

**Query parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| from | date (ISO 8601) | start of current month | Inclusive start date (by SessionDate) |
| to | date (ISO 8601) | today | Inclusive end date |
| employeeId | int | (none) | Filter by employee ID (any authenticated user may filter) |

**Response 200** (all authenticated users see all sessions):
```json
{
  "items": [
    {
      "id": 15,
      "employeeId": 3,
      "employeeName": "Anna Berg",
      "sessionDate": "2026-06-13",
      "startTime": "08:00",
      "endTime": "15:30",
      "durationMinutes": 450,
      "notes": null
    }
  ],
  "totalMinutes": 450
}
```

---

## POST /api/work-sessions

Logs a new work session for the authenticated employee.

**Request body**:
```json
{
  "sessionDate": "2026-06-13",
  "startTime": "08:00",
  "endTime": "15:30",
  "notes": "Covered morning service"
}
```

**Validation**:
- `sessionDate`: required, ISO 8601 date, not in the future
- `startTime`: required, HH:mm format
- `endTime`: required, HH:mm format, must be later than `startTime`
- `notes`: optional, max 500 characters

**Response 201**: Returns the created work session (same shape as a single item in GET).
**Response 400**: Validation error details.

---

## PUT /api/work-sessions/{id}

Replaces all editable fields of a work session. Any authenticated employee may edit any
session (no role restriction in current scope).

**Request body**: Same shape as POST.

**Response 200**: Returns the updated work session.
**Response 404**: Work session not found.
**Response 400**: Validation error details.

---

## DELETE /api/work-sessions/{id}

Deletes a work session. Any authenticated employee may delete any session (no role
restriction in current scope).

**Response 204**: Deleted successfully.
**Response 404**: Work session not found.
