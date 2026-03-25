# Feature 3.6: Dashboard Widget System (Spike)

## Goal

Replace the dashboard stub with a customizable widget grid powered by GridStack.js. Users can add, remove, rearrange, and resize widgets from a predefined catalog. Layout is persisted per user in PostgreSQL. Two demo widgets prove out the integration: total mail count and mails-per-day bar chart (Chart.js).

This is a spike — the focus is on proving the widget infrastructure, not on polishing widget content.

## Deliverables

### Data Model

New `DashboardLayout` entity for persisting widget grid state per user.

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `Guid` | Primary key |
| `UserId` | `Guid` | FK to `User`, unique (one layout per user) |
| `LayoutJson` | `string` (JSON column) | GridStack serialized array: `[{id, x, y, w, h, widgetType}]` |
| `UpdatedAt` | `DateTimeOffset` | Last modification timestamp |

### API

**Dashboard layout endpoints** (`DashboardEndpoints`, group `/api/dashboard`):

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/dashboard/layout` | Returns the authenticated user's layout JSON (or empty array if none saved) |
| `PUT` | `/api/dashboard/layout` | Saves the authenticated user's layout JSON |

**Mail statistics endpoint** (extension of mail domain, `MailStatsEndpoints`, group `/api/mail`):

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/mail/stats` | Returns total message count and mails-per-day for the last 7 days |

Response shape for `/api/mail/stats`:

```json
{
  "totalCount": 142,
  "mailsPerDay": [
    { "date": "2026-03-19", "count": 12 },
    { "date": "2026-03-20", "count": 8 },
    ...
  ]
}
```

DTOs in `Feirb.Shared`:
- `DashboardLayoutResponse` / `DashboardLayoutRequest` — wrapper around the JSON array
- `MailStatsResponse` — total count + mails per day
- `DailyMailCountItem` — date + count pair

### UI — Widget Grid

- Replace the dashboard stub (`Home.razor`) with a GridStack-powered widget grid
- **Locked by default** — widgets are not draggable/resizable in normal mode
- **Edit mode** toggled via a button in the `Toolbar` component:
  - Grid becomes draggable and resizable
  - Right sidebar slides in with the widget catalog
  - Each widget shows a small delete (X) button in its corner
- On exiting edit mode, the layout is serialized and saved via `PUT /api/dashboard/layout`
- On page load, layout is restored from `GET /api/dashboard/layout`
- Empty dashboard (no saved layout) shows an empty grid — no default layout for the spike

### UI — Widget Catalog Sidebar

- Right-side panel, visible only in edit mode
- Search bar at the top — filters widgets by localized name and description
- Lists all available widgets from the predefined catalog
- Clicking a widget adds it to the grid

### UI — Widget: Total Mail Count

- Displays a single number: total cached messages across all mailboxes
- Data from `GET /api/mail/stats` → `totalCount`
- Minimal styling — label + large number

### UI — Widget: Mails Per Day Chart

- Bar chart showing message count per day for the last 7 days
- Data from `GET /api/mail/stats` → `mailsPerDay`
- Rendered via Chart.js (raw JS interop, no wrapper library)
- X-axis: dates, Y-axis: message count

### JS Interop

- `gridstack-interop.js` — initializes GridStack, handles add/remove/serialize, relays drag-end and resize-end events back to Blazor via `DotNetObjectReference`
- `chartjs-interop.js` — renders and updates Chart.js bar charts in widget containers
- GridStack and Chart.js loaded via CDN references (vendoring deferred to a separate feature)

### Widget Registry

Predefined widget catalog, server-aware. Each widget type has:
- Internal identifier (e.g., `mail-count`, `mails-per-day`)
- Localization keys for name and description (used in sidebar search)
- Associated Blazor component

The registry is a static list in the frontend — no API endpoint for the catalog in the spike.

## Testing

- **API tests:** `DashboardEndpoints` — layout save/load round-trip, empty state returns empty array, user isolation (user A cannot read user B's layout)
- **API tests:** `MailStatsEndpoints` — returns correct total count, returns 7 days of data, handles zero-message state
- **Frontend tests:** Widget catalog search filters by localized name and description

## i18n

New resource keys (all 4 locales: `en-US`, `de-DE`, `fr-FR`, `it-IT`):

| Key | en-US | Purpose |
|-----|-------|---------|
| `DashboardEditButton` | Edit dashboard | Toolbar button |
| `DashboardEditDone` | Done | Exit edit mode button |
| `WidgetCatalogTitle` | Add widgets | Sidebar heading |
| `WidgetCatalogSearch` | Search widgets... | Search placeholder |
| `WidgetMailCountName` | Total emails | Widget catalog name |
| `WidgetMailCountDescription` | Total number of emails across all mailboxes | Widget catalog description / search text |
| `WidgetMailsPerDayName` | Emails per day | Widget catalog name |
| `WidgetMailsPerDayDescription` | Bar chart showing emails received per day over the last 7 days | Widget catalog description / search text |

## Out of Scope

- Default dashboard layout for new users
- Widget-level configuration (e.g., chart timeframe selection)
- Smart empty states (e.g., "Configure a mailbox first")
- JS dependency vendoring (separate feature)
- Security review (separate story)

## Dependencies

- Phase 3 features 1-3 (cached messages in the database, mail API)

## Acceptance Criteria

- [ ] Dashboard page displays a GridStack widget grid instead of the stub
- [ ] Edit mode is toggled from the toolbar; grid is locked in normal mode
- [ ] Widget catalog sidebar appears in edit mode with search functionality
- [ ] Search matches against localized widget name and description
- [ ] Total mail count widget displays the correct number from the API
- [ ] Mails-per-day widget renders a 7-day bar chart via Chart.js
- [ ] Widgets can be added from the catalog, removed via X button, dragged, and resized in edit mode
- [ ] Layout is persisted in PostgreSQL and restored on page load
- [ ] Layout is user-scoped — users cannot access each other's layouts
- [ ] All user-facing strings are localized in 4 locales
- [ ] `GET /api/mail/stats` returns total count and 7-day daily counts
- [ ] `GET /api/dashboard/layout` and `PUT /api/dashboard/layout` work correctly
- [ ] API and frontend tests pass
