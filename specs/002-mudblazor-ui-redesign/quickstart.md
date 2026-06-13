# Quickstart: MudBlazor UI Redesign Validation

## Prerequisites

- Docker running with SQL Server container: `docker-compose up -d`
- .NET 10 SDK installed
- Static Web Apps CLI: `npm install -g @azure/static-web-apps-cli`
- API running: `swa start` in project root

## Validation Scenarios

### Scenario 1: Navigation Consistency Across Pages

**Setup**: Application loaded at http://localhost:4280

**Steps**:
1. Open browser DevTools (F12) → **Console tab** — verify no errors
2. Click "Orders" in navigation → page loads without console errors
3. Click "Products" in navigation → same layout and navigation styling appears
4. Click "Expenses" in navigation (if available) → same layout
5. On mobile (375px viewport): Click hamburger icon → drawer slides in
6. On mobile: Click "Products" → drawer closes, page changes

**Expected Result**: 
- ✅ No console errors on any page
- ✅ Navigation layout identical on Orders, Products, Expenses pages
- ✅ Active nav link visually highlighted
- ✅ Mobile drawer opens/closes smoothly

---

### Scenario 2: Form Styling and Validation

**Setup**: Application at Orders page

**Steps**:
1. Scroll to ProductPicker section → inputs are MudTextField components
2. Click quantity input → focus state shows
3. Try submitting empty form (if applicable) → validation errors use MudBlazor error styling
4. Fill form correctly and submit → success feedback appears
5. Check Ledger page (if available) → date picker uses MudDatePicker

**Expected Result**:
- ✅ All form inputs have consistent MudBlazor appearance
- ✅ Error messages appear inline with red background/icon
- ✅ Success states use green/checkmark styling
- ✅ Date/time inputs use MudBlazor calendar/time pickers

---

### Scenario 3: Table Display and Data Presentation

**Setup**: Application at Products page

**Steps**:
1. Load Products page → product list displayed in MudTable
2. Verify columns: Name, Price, Status, Actions
3. Hover over table rows → row highlight appears
4. Click a column header (if sortable) → data sorts without page reload
5. Status badges show "Active" (green) or "Inactive" (red)

**Expected Result**:
- ✅ Table uses MudTable with striped rows (alternating colors)
- ✅ Column headers are bold and clearly distinguished
- ✅ Status badges use MudChip with color-coded states
- ✅ Sorting works without full page refresh

---

### Scenario 4: Mobile Responsiveness (375px Viewport)

**Setup**: Open DevTools → Device Toolbar → iPhone SE (375×667)

**Steps**:
1. Navigate to Orders page
2. Verify no horizontal scrolling of main content
3. Try to tap "Record Order" button → button is ≥44×44px (easily tappable)
4. Scroll through form inputs → all properly sized for touch
5. Open mobile navigation menu → menu fills available width
6. Check Products page at 375px → product table reflows vertically or scrolls within viewport

**Expected Result**:
- ✅ No horizontal scrolling of primary content
- ✅ All buttons/inputs ≥44×44px
- ✅ Form labels clearly associated with inputs
- ✅ Layout adapts gracefully to 375px width

---

### Scenario 5: No Bootstrap Classes

**Setup**: Open DevTools → Elements/Inspector tab

**Steps**:
1. Right-click on any button → Inspect → examine class attribute
2. Verify: No `btn`, `btn-primary`, `form-control`, `table`, `col-`, `row` classes
3. Spot-check 5 different components (button, input, table cell, card)
4. Search page source (Ctrl+F → "bootstrap") → zero matches

**Expected Result**:
- ✅ All inspected elements use MudBlazor classes only
- ✅ No Bootstrap CSS framework classes present
- ✅ Page source contains zero Bootstrap references

---

### Scenario 6: Dark Mode Toggle (Optional)

**Setup**: Application loaded; theme toggle visible (if implemented)

**Steps**:
1. Locate theme toggle in AppBar or settings
2. Click to switch to dark mode
3. Verify all elements remain readable and visible
4. Navigate to another page → theme persists
5. Reload page → theme preference restored

**Expected Result**:
- ✅ Dark mode activates without page reload
- ✅ All text contrast remains accessible (WCAG AA minimum)
- ✅ Colors adapt: backgrounds dark, text light
- ✅ Preference persists across navigation and reloads

---

## Test Devices / Viewports

| Device | Viewport | Priority |
|--------|----------|----------|
| Desktop (Chrome/Firefox) | 1920×1080 | P1 |
| Tablet | 768×1024 | P1 |
| Mobile (iPhone SE) | 375×667 | P1 |
| Mobile (iPhone 14) | 430×932 | P2 |
| Desktop (Safari) | 1440×900 | P2 |

## Regression Testing

**Ensure existing functionality still works**:
- [ ] Create product → appears in list
- [ ] Submit order → order stored, visible in list
- [ ] Edit expense → changes persist
- [ ] View ledger → orders and expenses sum correctly
- [ ] Log work session → duration calculated correctly
- [ ] All API calls succeed (check Network tab in DevTools)

## Success Criteria Verification Checklist

- [ ] Scenario 1: Navigation consistency — PASS on all pages
- [ ] Scenario 2: Form validation — error styling correct
- [ ] Scenario 3: Table display — striped rows, badges colored
- [ ] Scenario 4: Mobile (375px) — no horizontal scroll, touch targets ≥44×44px
- [ ] Scenario 5: Zero Bootstrap — no Bootstrap classes in DOM
- [ ] Scenario 6: Dark mode (if implemented) — persists across navigation
- [ ] Regression: All existing features work without errors
