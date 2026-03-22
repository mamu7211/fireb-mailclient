# Feature 2.6: Breadcrumb & Drill-Down Navigation

## Goal

Create reusable navigation components for settings and admin pages: a global breadcrumb, a drill-down section list, and a content section component. Wire up the settings and admin page hierarchy.

## Deliverables

### Breadcrumb Component

- Global, rendered on all authenticated pages
- All segments are clickable links
- Root segment is "Dashboard" linking to `/`
- Route metadata registry maps path segments to localized labels
- Dynamic segments (e.g., GUIDs) resolved via API (for future use)
- Supports arbitrary nesting depth

### Drill-Down Navigation Item Component

- Reusable row: icon + title + subtitle + chevron
- Clicking navigates to a sub-route
- Used on "menu pages" (intermediate levels in the hierarchy)

### Content Section Component

- Same header style as navigation item (icon + title + subtitle)
- Renders arbitrary content below the header (forms, toggles, etc.)
- Used on "leaf pages" for visually separated sections
- No navigation behavior

### Settings Page (`/settings`)

- Section list with one entry: Account Settings
- Account Settings shows two navigation items: Personal Information, Security & Password
- Both link to stub pages for now (implemented in Feature 2.7)

### Admin Page (`/admin`)

- Section list with three entries: User Management, System Settings, LLM Settings
- User Management links to the existing users table at `/admin/user-management`
- System Settings drills down to: Outgoing SMTP (stub), Jobs (stub)
- LLM Settings drills down to: System Prompt (stub), Scheduling (stub)

### Route Structure

```
/settings                                  → Section list
/settings/personal-information             → Stub
/settings/security                         → Stub

/admin                                     → Section list
/admin/user-management                     → Existing users table
/admin/system-settings                     → Sub-section list
/admin/system-settings/outgoing-smtp       → Stub
/admin/system-settings/jobs                → Stub
/admin/llm-settings                        → Sub-section list
/admin/llm-settings/system-prompt          → Stub
/admin/llm-settings/scheduling             → Stub
```

### Stitch Reference

- Project: "Login Page"
- Screen: "Settings Page (Vertical Layout)" — shows the drill-down pattern with section headers, navigation items, and search box

## i18n

All new strings (section titles, subtitles, breadcrumb labels) localized in `en-US`, `de-DE`, `fr-FR`, `it-IT`.

## Acceptance Criteria

- [ ] Breadcrumb renders on all authenticated pages with clickable segments
- [ ] Dashboard is the breadcrumb root
- [ ] Drill-down navigation works at arbitrary depth
- [ ] Settings page shows Account Settings section
- [ ] Admin page shows all three sections with correct hierarchy
- [ ] Existing user management table accessible via new route
- [ ] Stub pages render with correct breadcrumb
- [ ] All strings localized in 4 locales
- [ ] Components are reusable (shared between Settings and Admin)
