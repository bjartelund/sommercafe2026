# Feature Specification: MudBlazor UI Redesign

**Feature Branch**: `002-mudblazor-ui-redesign`

**Created**: 2026-06-13

**Status**: Draft

**Input**: User description: "Amend existing specification: Replace default Blazor Bootstrap-based UI. Use MudBlazor for all UI components. Ensure consistent layout with top navigation and drawer. No Bootstrap usage. Scope: UI layer only. Do not change backend or API design"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Consistent Navigation Experience (Priority: P1)

Employees navigate between Orders, Products, Expenses, Ledger, and Work Hours pages using a persistent, cohesive navigation structure. The interface provides immediate visual feedback on the current page and quick access to all application features without layout shifts or inconsistent styling.

**Why this priority**: Navigation is the first thing users interact with; a consistent, professional experience across all pages builds confidence in the application. Poor navigation is the most common source of user frustration.

**Independent Test**: Can be fully tested by navigating between all main pages (Orders, Products, Expenses, Ledger, Work Hours) and verifying the same layout, visual style, and navigation patterns appear consistently on every page.

**Acceptance Scenarios**:

1. **Given** a user is on any main page, **When** they click a navigation link, **Then** the page changes and the navigation element for the new page is visually highlighted as active
2. **Given** a user opens the application, **When** the page loads on a mobile or tablet viewport, **Then** the navigation drawer collapses to a hamburger icon and can be toggled open/closed
3. **Given** a user is viewing the application on a desktop viewport, **When** they interact with the UI, **Then** the navigation drawer remains visible as a persistent sidebar

---

### User Story 2 - Professional Data Display (Priority: P1)

Employees view products, orders, expenses, and ledger entries in clean, scannable tables and cards. Data is presented with consistent typography, spacing, and visual hierarchy. Forms for creating and editing records feel modern and intuitive with clear labeling and validation feedback.

**Why this priority**: The application's primary value comes from displaying and managing data. Professional, consistent presentation of data increases trust and improves task completion speed.

**Independent Test**: Can be fully tested by viewing the Products listing page and verifying that product names, prices, status badges, and action buttons follow consistent design patterns with proper spacing and visual hierarchy.

**Acceptance Scenarios**:

1. **Given** a user views a data table or list, **When** they scan the page, **Then** column headers are clearly distinguished, rows alternate colors or have clear separation, and data values are aligned consistently
2. **Given** a user opens a form to create or edit a record, **When** they interact with inputs, **Then** field labels are prominent, validation errors appear inline with clear messaging, and form actions (Save/Cancel) are obvious and properly sized
3. **Given** a user hovers over or clicks an interactive element, **When** they perform the action, **Then** visual feedback (color change, shadow, ripple effect) confirms the interaction is registered

---

### User Story 3 - Touch-Friendly Mobile Experience (Priority: P1)

Employees using mobile or tablet devices can comfortably interact with all application features. Buttons, form inputs, and interactive elements are appropriately sized for touch interaction. The interface adapts responsively to different screen sizes without requiring horizontal scrolling for primary content.

**Why this priority**: Employees may log orders, expenses, and work hours from the field. A poor mobile experience on-site will frustrate users and reduce adoption.

**Independent Test**: Can be fully tested by viewing the application at 375px viewport width (iPhone SE), navigating between pages, submitting a form, and verifying that all interactive elements are easily tappable and no horizontal scrolling is required for primary content.

**Acceptance Scenarios**:

1. **Given** a user is on a mobile device (375px–667px viewport), **When** they attempt to tap a button or form input, **Then** all interactive elements are at least 44×44 pixels and properly spaced from neighbors
2. **Given** a user is viewing content on a mobile device, **When** the content exceeds the viewport width, **Then** no horizontal scrolling is required for the main content area; only secondary content (if any) scrolls horizontally
3. **Given** a user is on a tablet or desktop, **When** they interact with the interface, **Then** the layout reflows gracefully without jarring layout shifts or element reorganization

---

### User Story 4 - Consistent Component Styling (Priority: P2)

All UI components—buttons, text inputs, selects, modals, cards, badges—use MudBlazor's standard appearance and behavior. Color schemes, borders, spacing, and interaction patterns are consistent across all pages. Form validation states (success, error, loading) are visually distinct and predictable.

**Why this priority**: Visual consistency reinforces professionalism and reduces user confusion. Inconsistent styling makes the application feel unmaintained or fragmented.

**Independent Test**: Can be fully tested by creating a visual audit of all component types used across Orders, Products, and Expenses pages, verifying that the same component type (e.g., buttons, input fields, badges) has identical styling, spacing, and behavior across all uses.

**Acceptance Scenarios**:

1. **Given** a user performs an action that produces an error, **When** the error is displayed, **Then** it appears in a consistent color/style (e.g., red background with icon) across all pages
2. **Given** a user interacts with a success state (e.g., order submitted, product created), **When** confirmation is displayed, **Then** it uses consistent success styling (e.g., green background) across all pages
3. **Given** a user views a form or data table on any page, **When** they examine spacing, typography, and alignment, **Then** the spacing between elements, text sizes, and element alignment match the same page type on other pages

---

### User Story 5 - Dark/Light Theme Support (Priority: P3)

The application supports both light and dark color schemes. Employees can toggle between themes, and their preference is persisted across sessions. All components automatically adapt to the selected theme without requiring page reload.

**Why this priority**: Theme support improves accessibility and user comfort during extended use. It's a quality-of-life improvement that demonstrates attention to detail.

**Independent Test**: Can be fully tested by toggling a theme switcher between light and dark modes, navigating to different pages, and verifying that all UI elements are visible, readable, and properly colored in both themes.

**Acceptance Scenarios**:

1. **Given** a user is on any page, **When** they toggle the theme switcher, **Then** the entire interface immediately switches to the opposite theme
2. **Given** a user has set a theme preference, **When** they reload the page or return to the application later, **Then** their previous theme preference is restored

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: All Bootstrap CSS imports MUST be removed from `src/Client/wwwroot/index.html` and `src/Client/Client.csproj`
- **FR-002**: MudBlazor NuGet package MUST be added to `src/Client/Client.csproj`
- **FR-003**: MudBlazor MUST be initialized in `src/Client/Program.cs` with `services.AddMudServices()`
- **FR-004**: All Razor pages and components MUST be refactored to use MudBlazor components (MudTable, MudButton, MudTextField, MudCard, etc.) instead of Bootstrap HTML elements
- **FR-005**: Navigation layout MUST use MudAppBar (top bar) and MudDrawer (sidebar) to create a persistent, responsive navigation pattern
- **FR-006**: Mobile navigation MUST collapse the drawer to a hamburger menu on viewports narrower than 960px
- **FR-007**: All form inputs MUST use MudBlazor input components (MudTextField, MudNumericField, MudSelect, MudDatePicker, etc.)
- **FR-008**: All buttons MUST use MudButton with appropriate Variant (Text, Outlined, Filled) and Color attributes
- **FR-009**: All tables MUST use MudTable with striped rows, proper column alignment, and sortable column headers
- **FR-010**: All status indicators and badges MUST use MudChip with appropriate Color (Success, Error, Warning, Info)
- **FR-011**: All modal dialogs and overlays MUST use MudDialog instead of custom modal HTML
- **FR-012**: All form validation error messages MUST be displayed using MudField's HelperText or Error properties
- **FR-013**: Loading states MUST use MudProgressLinear or MudSkeleton for data-fetching operations
- **FR-014**: Spacing, typography, and color palette MUST be consistent across all pages and components
- **FR-015**: The application MUST NOT contain any Bootstrap class names or utility classes in view files after refactoring

### Key Entities

- **UI Component**: Represents a self-contained, reusable visual element (e.g., ProductForm, OrderSummary, NavMenu). Refactoring updates the internal implementation without changing the component's public interface (parameters and EventCallbacks).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of .razor page files contain zero Bootstrap class references and zero Bootstrap HTML elements; all interactive elements use MudBlazor components
- **SC-002**: Navigation is consistent across all 5 main pages (Orders, Products, Expenses, Ledger, Work Hours) with identical visual appearance, spacing, and behavior
- **SC-003**: All form inputs display validation errors using consistent MudBlazor error styling (color, icon, text placement)
- **SC-004**: The application is responsive and displays correctly at 375px (mobile), 768px (tablet), and 1920px (desktop) viewport widths with no horizontal scrolling of primary content
- **SC-005**: All interactive elements (buttons, inputs, links) are at least 44×44 pixels in size on mobile viewports
- **SC-006**: The application loads and renders without errors on all pages (verified through Blazor developer console with zero console errors)
- **SC-007**: All existing functionality (create product, submit order, edit expense, etc.) works identically after refactoring—no regressions in feature behavior

## Assumptions

- **Frontend-only scope**: The API backend, database schema, and Azure Functions remain unchanged. Only the Blazor client presentation layer is refactored.
- **No behavior changes**: Component refactoring updates the visual presentation; all business logic, validation, and API calls remain identical.
- **MudBlazor version**: MudBlazor v6.x or later is compatible with .NET 10 and Blazor WASM hosting model assumed.
- **No external styling**: All styling comes from MudBlazor theme; custom CSS is minimized (MudBlazor's theme system is the primary styling approach).
- **Persistent navigation model**: The top app bar + drawer pattern is the chosen persistent navigation paradigm; tab-based or bottom navigation is out of scope.
- **Existing component structure preserved**: Current component hierarchy (pages → composite components → leaf components) remains unchanged; refactoring updates component internals, not their structure or public APIs.
- **Browser compatibility**: MudBlazor targets modern browsers (Chrome, Firefox, Safari, Edge) with no legacy IE support required.
