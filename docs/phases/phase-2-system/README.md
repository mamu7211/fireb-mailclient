# Phase 2: System Setup & Administration

## Features (in implementation order)

| # | Feature | Issue | Description |
|---|---------|-------|-------------|
| 1 | [System Setup](01-system-setup.md) | #18 | Initial setup wizard: admin account creation, SMTP configuration |
| 2 | [Admin Account](02-admin-account.md) | #19 | IsAdmin flag, admin authorization policy, JWT admin claim |
| 3 | [User Management](03-user-management.md) | #20 | Admin page for managing user accounts |
| 4 | [Password Reset Mail](04-password-reset-mail.md) | #21 | Send reset link via SMTP instead of console log |
| 5 | [Main Page](05-main-page.md) | #22 | Authenticated landing page with right-side navbar |
| 6 | [Breadcrumb & Navigation](06-breadcrumb-navigation.md) | #38 | Breadcrumb component, drill-down navigation, settings/admin page hierarchy |
| 7 | [Profile Editing](07-profile-editing.md) | #39 | Personal Information & Security & Password pages with API |
| 8 | [Dashboard Landing](08-dashboard-landing.md) | #40 | Dashboard stub at `/`, mail route stubs, sidebar updates |
| 9 | [Mailbox Configuration](09-mailbox-configuration.md) | #42 | Per-user IMAP/SMTP mailbox setup with connection testing |
| 10 | [Outgoing SMTP Settings](10-outgoing-smtp-settings.md) | #56 | Admin page to edit system SMTP after setup |

## Dependencies

- Phase 1 (Auth) must be complete — login, registration, password reset working
- Feature 1 is the foundation for all other features in this phase
- Feature 2 depends on Feature 1 (first admin created during setup)
- Feature 3 depends on Feature 2 (admin role required)
- Feature 4 depends on Feature 1 (SMTP configuration from setup)
- Feature 5 depends on Feature 2 (admin navigation entries)
- Feature 6 depends on Feature 5 (main page layout exists)
- Feature 7 depends on Feature 6 (breadcrumb and navigation components)
- Feature 8 should be implemented before Feature 6 (Dashboard is the breadcrumb root)
- Feature 9 depends on Feature 6 (drill-down navigation, toolbar) and #41 (Data Protection fix)
- Feature 10 depends on Feature 1 (SmtpSettings entity created during setup)
