# Feature 2.8: Dashboard Landing Page

## Goal

Introduce a Dashboard stub as the application root (`/`) and create stub pages for all mail routes. This establishes the navigation foundation and provides the breadcrumb root for Feature 2.6.

**Related:** Feature 2.6 (Breadcrumb & Navigation — Dashboard is the breadcrumb root)

## Deliverables

### Dashboard Page (`/`)

- Replaces current `Home.razor` welcome page
- Stub content: heading + "Coming soon" subtitle
- Breadcrumb shows `Dashboard` with no link (it's the root)

### Mail Stub Pages

- `/mail/inbox`
- `/mail/sent`
- `/mail/drafts`
- `/mail/trash`
- `/mail/archive`
- All show heading + "Coming soon" subtitle

### Sidebar Navigation

- Add "Dashboard" entry to NavMenu
- Update all mail entries to point to `/mail/...` routes

### Shared Resource Key

- `ComingSoon` resource key for reuse across all stub pages
- Replace hardcoded "Coming soon" in existing Settings page

## i18n

All new strings localized in `en-US`, `de-DE`, `fr-FR`, `it-IT`.

## Acceptance Criteria

- [ ] `/` shows Dashboard stub
- [ ] All mail stub pages render at `/mail/...` routes
- [ ] Sidebar entries point to correct routes
- [ ] "Dashboard" entry in sidebar
- [ ] `ComingSoon` resource key used consistently
- [ ] All strings localized in 4 locales
