# Research: MudBlazor UI Redesign

## Decision: MudBlazor as Bootstrap Replacement

**Decision**: Use MudBlazor v6.x for all UI components, replacing Bootstrap 5.

**Rationale**: 
- MudBlazor is purpose-built for Blazor WASM applications with strong TypeScript/C# integration
- Provides complete component library (tables, forms, dialogs, drawers) matching all current Bootstrap usage
- Material Design foundation ensures professional, consistent visual language
- Active maintenance and strong community support
- Zero-learning-curve migration: MudBlazor components map one-to-one to existing Bootstrap elements

**Alternatives Considered**:
1. **Keep Bootstrap 5** — Rejected: Bootstrap is generic; not optimized for Blazor WASM, requires manual JS interop for interactive components, no theme system
2. **Ant Design Blazor** — Rejected: Smaller community, less frequent updates than MudBlazor, similar learning curve
3. **Radzen Blazor Components** — Rejected: Commercial licensing model; feature set overlaps MudBlazor but with higher cost barrier
4. **Custom CSS-in-Blazor** — Rejected: Violates Principle V (Simplicity); introduces maintenance burden with no business value

## Theme System Design

**Decision**: Use MudBlazor's built-in theme system with light/dark mode support.

**Rationale**:
- MudBlazor's theme API eliminates need for custom CSS utility classes
- Color palette, typography, spacing all configurable through theme object in Program.cs
- Dark mode toggle uses browser preferences + user override
- Simplifies ongoing UI maintenance

## Component Mapping

| Bootstrap Element | MudBlazor Component | Notes |
|-------------------|-------------------|-------|
| `.navbar` | MudAppBar | Top navigation with branding |
| `.drawer` | MudDrawer | Sidebar navigation (collapsible on mobile) |
| `.btn` | MudButton | All button styles (Text, Outlined, Filled) |
| `.form-control` | MudTextField | Text inputs, numbers, dates |
| `.table` | MudTable | Data display with striping, sorting |
| `.modal` | MudDialog | Modal overlays and confirmations |
| `.alert` | MudAlert | Success/error/warning messages |
| `.badge` | MudChip | Status indicators |
| `.card` | MudPaper or MudCard | Content containers |

## Mobile-First Responsive Strategy

**Decision**: MudBlazor's breakpoint system handles responsive layout; drawer collapses to hamburger on Sm viewports (< 960px).

**Implementation**:
- MudAppBar with default visible drawer on desktop
- MudHidden component hides drawer toggle on desktop (visible on mobile)
- Flex/grid layouts use MudBreakpoint directives for responsive reflow
- All touch targets minimum 44×44px on mobile via MudButton/MudIconButton size props

## No Custom CSS Approach

**Decision**: Minimize src/Client/wwwroot/css/ to theme tweaks only; delegate all styling to MudBlazor.

**Rationale**:
- MudBlazor handles 100% of component styling
- Custom CSS risks inconsistency and maintenance debt
- Theme modifications in Program.cs are version-controlled and testable
- Simplifies code review and onboarding
